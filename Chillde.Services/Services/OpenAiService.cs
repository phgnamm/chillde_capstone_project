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
            string prompt = GeneratePrompt(attributes);

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                new { role = "system", content = "You are an AI that converts a list of attributes into a structured model with type and options." },
                new { role = "user", content = prompt }
            },
                max_tokens = 300,
                temperature = 0.3
            };
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseString);
            string jsonResponse = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            var result = JsonSerializer.Deserialize<List<ModelResponse>>(jsonResponse);
            return new ResponseModel { Data = result };
        }

        private string GeneratePrompt(List<string> prompt)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Based on the given item category and description, generate structured JSON defining relevant attributes for handmade products.");
            sb.AppendLine("Category: " + prompt[0]);
            sb.AppendLine("Description: " + prompt[1]);
            sb.AppendLine();
            sb.AppendLine("The output should be a list of attributes, where each attribute has:");
            sb.AppendLine("- `name` (string): The attribute name.");
            sb.AppendLine("- `type` (string): Choose one of:");
            sb.AppendLine("  - 'text': Free-text input (e.g., product description).");
            sb.AppendLine("  - 'number': Numerical values (e.g., price, weight).");
            sb.AppendLine("  - 'dropdown': Predefined choices (e.g., material, color).");
            sb.AppendLine("  - 'boolean': Yes/No options (e.g., 'Is customizable?').");
            sb.AppendLine("  - 'image': Product images.");
            sb.AppendLine("  - 'file': Uploadable files (e.g., design files, templates).");
            sb.AppendLine("  - 'date': Date-related attributes.");
            sb.AppendLine("  - 'multiselect': Multiple selections (e.g., suitable occasions).");
            sb.AppendLine("- `options` (list of strings, only for 'dropdown' and 'multiselect').");
            sb.AppendLine("Ensure that:");
            sb.AppendLine("- The attributes are relevant to the handmade category.");
            sb.AppendLine("- 'options' include common values for that category.");
            sb.AppendLine("- The response is valid JSON in a list format.");

            return sb.ToString();
        }

        public class OpenAiEmbeddingResponse
        {
            public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();
        }

        public class EmbeddingData
        {
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
        public class ModelResponse
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public List<string> Options { get; set; }
        }

    }
}
