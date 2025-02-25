using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.Interfaces;
using Microsoft.Extensions.Options;
using Chillde.Repositories.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;

namespace Chillde.Services.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IConfiguration _configuration;
        private readonly IOpenAIService _openAiService;
        private readonly ModelConfigurationOptions _modelConfigurationOptions;

        public OpenAiService(IConfiguration configuration, IOpenAIService openAiService,
            IOptions<ModelConfigurationOptions> modelConfigurationOptions
)
        {
            _openAiService = openAiService;
            _modelConfigurationOptions = modelConfigurationOptions.Value;
            _configuration = configuration;
        }

        public async Task<ResponseModel> GetEventAsync(string sourceLanguage, string targerLanguage)
        {
            var fineTunedModel = _modelConfigurationOptions.FineTunedModelId;
            var defaultModel = _modelConfigurationOptions.DefaultModel;
            var eventPromptEn = "What is the upcoming events in Vietnam within the next 1.5 months? Give one event nearliest (just give the name of event only)";
            var eventPromptVi = "Trong vòng 1,5 tháng tới, có sự kiện gì ở Việt Nam? Hãy cho biết một sự kiện gần nhất (chỉ cần cho tên sự kiện)";

            var prompt = targerLanguage == "en" ? eventPromptEn : eventPromptVi;

            var eventResponse = await _openAiService.Completions.CreateCompletion(
                new CompletionCreateRequest
                {
                    Prompt = prompt,
                    Model = defaultModel,
                    MaxTokens = 100
                });

            string? eventInfo = eventResponse?.Choices?.FirstOrDefault()?.Text?.Trim();
            if (string.IsNullOrEmpty(eventInfo))
            {
                eventInfo = "No upcoming events found.";
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = eventInfo,
                Data = eventInfo
            };
        }

        public async Task<float[]> GetEmbeddingAsync(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
            {
                throw new ArgumentException("Input cannot be empty.", nameof(texts));
            }

            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "text-embedding-ada-002",
                input = texts
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/embeddings", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve embeddings: {response.StatusCode} - {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>();
            return responseData?.Data.FirstOrDefault()?.Embedding ?? throw new Exception("No embedding data returned.");
        }

        public async Task<ResponseModel> GetStructuredDataAsync(List<string> attributes)
        {
            if (attributes == null || attributes.Count < 2)
                throw new ArgumentException("Attributes list must contain at least a category and a description.");

            string prompt = GeneratePrompt(attributes);

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                new { role = "system", content = "You are an AI that converts a list of attributes into a structured model with type and options." },
                new { role = "user", content = prompt }
            },
                max_tokens = 500,
                temperature = 0.3
            };

            string apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + responseString);

            using JsonDocument doc = JsonDocument.Parse(responseString);
            string jsonResponse = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (string.IsNullOrWhiteSpace(jsonResponse))
                throw new Exception("API response is empty.");

            try
            {
                using JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);
                if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
                    throw new Exception("Invalid JSON format: Expected list of attributes.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Exception($"Error parsing JSON: {ex.Message}\nResponse: {jsonResponse}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, 
                ReadCommentHandling = JsonCommentHandling.Skip, 
                AllowTrailingCommas = true
            };

            List<ModelResponseRaw> rawResult = System.Text.Json.JsonSerializer.Deserialize<List<ModelResponseRaw>>(jsonResponse, options);

            if (rawResult == null || rawResult.Count == 0)
                throw new Exception("Response data is null or empty.");

            var result = rawResult.ConvertAll(item => new ModelResponse
            {
                Name = item?.Name ?? "Unknown",
                Type = item?.Type != null ? ParseMediaType(item.Type) : MediaType.Text,
                Options = item?.Options ?? new List<string>()
            });

            return new ResponseModel { Data = rawResult };
        }

        private string GeneratePrompt(List<string> prompt)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Given an item category and description, analyze the provided information and generate a structured JSON output listing all possible and reasonable attributes for handmade products.");
            sb.AppendLine($"Category: {prompt[0]}");
            sb.AppendLine($"Description: {prompt[1]}");
            sb.AppendLine();
            sb.AppendLine("For each attribute, provide:");
            sb.AppendLine("- name (string): The attribute name.");
            sb.AppendLine("- type (integer), where:");
            sb.AppendLine("  - 0: Text (e.g., product description).");
            sb.AppendLine("  - 1: Number (e.g., price, weight).");
            sb.AppendLine("  - 2: Select (e.g., material, color).");
            sb.AppendLine("  - 3: Switch (Yes/No, e.g., 'Is customizable?').");
            sb.AppendLine("  - 4: Image (e.g., product photos).");
            sb.AppendLine("  - 5: File (e.g., design files, templates).");
            sb.AppendLine("  - 6: Checkbox (Multiple selections, e.g., suitable occasions).");
            sb.AppendLine("- options (list of strings, required for types 2 and 6, containing common or relevant values).");
            sb.AppendLine();
            sb.AppendLine("Ensure that the listed attributes are contextually relevant to handmade products, considering both general and specific aspects of the given category and description. The output should be a well-structured JSON array.");

            return sb.ToString();
        }


        private MediaType ParseMediaType(int type)
        {
            return type switch
            {
                0 => MediaType.Text,
                1 => MediaType.Number,
                2 => MediaType.Select,
                3 => MediaType.Switch,
                4 => MediaType.Image,
                5 => MediaType.File,
                6 => MediaType.CheckBox,
                _ => throw new ArgumentException($"Unknown media type: {type}")
            };
        }

        public class OpenAiEmbeddingResponse
        {
            public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();
        }

        public class EmbeddingData
        {
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }       
        public class ModelResponseRaw
        {
            public string Name { get; set; }
            public int Type { get; set; } 
            public List<string> Options { get; set; }
        }


        public class ModelResponse
        {
            public string Name { get; set; }
            public MediaType Type { get; set; }
            public List<string> Options { get; set; }
        }

    }
}
