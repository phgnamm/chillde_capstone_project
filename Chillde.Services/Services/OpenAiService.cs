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
using System.Globalization;
using Chillde.Repositories.Models.UserActivityLogModels;

namespace Chillde.Services.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IConfiguration _configuration;

        public OpenAiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public async Task<ResponseModel> GetEventAsync(string sourceLanguage, string targetLanguage)
        //{
        //    var defaultModel = _modelConfigurationOptions.DefaultModel;

        //    var currentDate = DateTime.Now.ToString("dd/MM/yyyy");
        //    var eventPromptEn = $"From {currentDate}, return ONLY the nearest holiday or major event in Vietnam within the next 1.5 months. DO NOT mention any other events. Only return the name of the nearest event. Example: Women's Day, Tet Holiday.";
        //    var eventPromptVi = $"Từ ngày {currentDate}, chỉ trả về ngày lễ hoặc sự kiện gần nhất ở Việt Nam trong vòng 1,5 tháng tới. KHÔNG liệt kê sự kiện khác. Chỉ trả về tên sự kiện gần nhất. Ví dụ: Ngày Phụ nữ Việt Nam, Tết Nguyên Đán.";
        //    var prompt = sourceLanguage == "vi" ? eventPromptVi : eventPromptEn;

        //    var eventResponse = await _openAiService.ChatCompletion.CreateCompletion(
        //        new ChatCompletionCreateRequest
        //        {
        //            Model = defaultModel,
        //            Messages = new List<ChatMessage>
        //            {
        //                ChatMessage.FromSystem("You are an AI that provides the nearest upcoming event in Vietnam, prioritizing the closest one."),
        //                ChatMessage.FromUser(prompt)
        //            },
        //            MaxTokens = 20,
        //            Temperature = 0.1f
        //        });

        //    string? eventInfo = eventResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

        //    if (string.IsNullOrEmpty(eventInfo))
        //    {
        //        eventInfo = "No upcoming events found.";
        //    }

        //    return new ResponseModel
        //    {
        //        Code = StatusCodes.Status200OK,
        //        Message = "Get Configuration Successfully",
        //        Data = eventInfo
        //    };
        //}

        public ResponseModel GetEvent(string sourLanguageCode)
        {
            var currentDate = DateTime.Now;
            var eventList = new List<EventModel>
                {
                    new EventModel { NameVi = "Tết Dương lịch", NameEn = "New Year's Day", Date = "01-01", IsLunar = false },
                    new EventModel { NameVi = "Ngày Thầy thuốc Việt Nam", NameEn = "Vietnamese Doctors' Day", Date = "27-02", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Phụ nữ", NameEn = "International Women's Day", Date = "08-03", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Hạnh phúc", NameEn = "International Day of Happiness", Date = "20-03", IsLunar = false },
                    new EventModel { NameVi = "Giỗ Tổ Hùng Vương", NameEn = "Hung Kings' Commemoration Day", Date = "10-03", IsLunar = true },
                    new EventModel { NameVi = "Ngày Giải phóng miền Nam", NameEn = "Reunification Day", Date = "30-04", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Lao động", NameEn = "International Workers' Day", Date = "01-05", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Thiếu nhi", NameEn = "International Children's Day", Date = "01-06", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Người cao tuổi", NameEn = "International Day of Older Persons", Date = "01-10", IsLunar = false },
                    new EventModel { NameVi = "Ngày Giải phóng Thủ đô", NameEn = "Hanoi Liberation Day", Date = "10-10", IsLunar = false },
                    new EventModel { NameVi = "Halloween", NameEn = "Halloween", Date = "31-10", IsLunar = false },
                    new EventModel { NameVi = "Ngày Nhà giáo Việt Nam", NameEn = "Vietnamese Teachers' Day", Date = "20-11", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quốc tế Nam giới", NameEn = "International Men's Day", Date = "19-11", IsLunar = false },
                    new EventModel { NameVi = "Ngày Quân đội Nhân dân Việt Nam", NameEn = "Vietnam People's Army Day", Date = "22-12", IsLunar = false },
                    new EventModel { NameVi = "Ngày Giáng sinh (Noel)", NameEn = "Christmas Eve", Date = "24-12", IsLunar = false },
                    new EventModel { NameVi = "Tết Nguyên Đán", NameEn = "Lunar New Year", Date = "01-01", IsLunar = true },
                    new EventModel { NameVi = "Lễ Vu Lan", NameEn = "Vu Lan Festival", Date = "15-07", IsLunar = true },
                    new EventModel { NameVi = "Tết Nguyên Tiêu", NameEn = "Lantern Festival", Date = "15-01", IsLunar = true },
                    new EventModel { NameVi = "Tết Đoan Ngọ", NameEn = "Dragon Boat Festival", Date = "05-05", IsLunar = true },
                    new EventModel { NameVi = "Tết Trung Thu", NameEn = "Mid-Autumn Festival", Date = "15-08", IsLunar = true },
                    new EventModel { NameVi = "Quốc khánh Việt Nam", NameEn = "Vietnam National Day", Date = "02-09", IsLunar = false },
                };

            var upcomingEvents = eventList
                .Select(e =>
                {
                    DateTime eventDate;
                    if (e.IsLunar)
                    {
                        var dateParts = e.Date!.Split('-');
                        int lunarDay = int.Parse(dateParts[0]);
                        int lunarMonth = int.Parse(dateParts[1]);
                        int lunarYear = currentDate.Year;
                        var chineseCalendar = new ChineseLunisolarCalendar();
                        eventDate = chineseCalendar.ToDateTime(lunarYear, lunarMonth, lunarDay, 0, 0, 0, 0);
                    }
                    else
                    {
                        eventDate = DateTime.ParseExact(e.Date!, "dd-MM", CultureInfo.InvariantCulture);
                        eventDate = new DateTime(currentDate.Year, eventDate.Month, eventDate.Day);
                    }
                    return new { Event = sourLanguageCode == "vi" ? e.NameVi : e.NameEn, Date = eventDate };
                })
                .Where(e => e.Date >= currentDate)
                .OrderBy(e => e.Date)
                .ToList();

            string nearestEvent = upcomingEvents.Any() ? upcomingEvents.First().Event! : (sourLanguageCode == "vi" ? "Không có sự kiện nào sắp diễn ra." : "No upcoming events.");

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = sourLanguageCode == "vi" ? "Lấy sự kiện thành công" : "Get Event Successfully",
                Data = nearestEvent
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
