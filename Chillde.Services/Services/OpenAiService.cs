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

        public ResponseModel GetEvent(string sourLanguageCode)
        {
            var currentDate = DateTime.Now;
            int gracePeriodDays = 3;
            int maxDaysThreshold = 45;
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
            return new { EventVi = e.NameVi, EventEn = e.NameEn, Date = eventDate };
        })
        .OrderBy(e => e.Date)
        .ToList();

            var nearestEvent = upcomingEvents.FirstOrDefault(e => e.Date >= currentDate);
            var pastRecentEvent = upcomingEvents.LastOrDefault(e => e.Date < currentDate);

            if (pastRecentEvent != null && (currentDate - pastRecentEvent.Date).TotalDays <= gracePeriodDays)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = sourLanguageCode == "vi" ? "Lấy sự kiện thành công" : "Get Event Successfully",
                    Data = new { pastRecentEvent.EventVi, pastRecentEvent.EventEn, Date = pastRecentEvent.Date.ToString("yyyy-MM-dd") }
                };
            }
            if (nearestEvent != null && (nearestEvent.Date - currentDate).TotalDays <= maxDaysThreshold)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = sourLanguageCode == "vi" ? "Lấy sự kiện thành công" : "Get Event Successfully",
                    Data = new { nearestEvent.EventVi, nearestEvent.EventEn, Date = nearestEvent.Date.ToString("yyyy-MM-dd") }
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = sourLanguageCode == "vi" ? "Không có sự kiện nào sắp diễn ra." : "No upcoming events.",
                Data = null
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
                max_tokens = 1000,
                temperature = 0.5
            };

            string apiKey = _configuration["OpenAI:ApiKey"]!;
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
            string jsonResponse = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;

            if (string.IsNullOrWhiteSpace(jsonResponse))
                throw new Exception("API response is empty.");

            // ✅ Trích xuất phần JSON array từ phản hồi
            int startIndex = jsonResponse.IndexOf('[');
            int endIndex = jsonResponse.LastIndexOf(']');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
                throw new Exception($"Could not extract valid JSON array from response.\nResponse: {jsonResponse}");

            string jsonArrayString = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);

            try
            {
                using JsonDocument jsonDoc = JsonDocument.Parse(jsonArrayString);
                if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
                    throw new Exception("Invalid JSON format: Expected list of attributes.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Exception($"Error parsing JSON: {ex.Message}\nRaw Extracted JSON: {jsonArrayString}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            List<ModelResponseRaw> rawResult = System.Text.Json.JsonSerializer.Deserialize<List<ModelResponseRaw>>(jsonArrayString, options)!;

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
            sb.AppendLine("Dựa vào danh mục sản phẩm và mô tả, hãy phân tích thông tin được cung cấp và tạo ra một đầu ra JSON có cấu trúc, liệt kê tất cả các thuộc tính hợp lý và chi tiết nhất cho sản phẩm thủ công.");
            sb.AppendLine($"Danh mục: {prompt[0]}");
            sb.AppendLine($"Mô tả: {prompt[1]}");
            sb.AppendLine();
            sb.AppendLine("Đối với mỗi thuộc tính, vui lòng cung cấp:");
            sb.AppendLine("- tên (string): Tên thuộc tính, viết bằng ngôn ngữ đơn giản và dễ hiểu cho khách hàng.");
            sb.AppendLine("- loại (integer), trong đó:");
            sb.AppendLine("  - 0: Văn bản (ví dụ: mô tả sản phẩm).");
            sb.AppendLine("  - 1: Số (ví dụ: giá, trọng lượng, kích thước).");
            sb.AppendLine("  - 2: Lựa chọn (ví dụ: chất liệu, màu sắc, kiểu dáng, tùy chọn Có/Không).");
            sb.AppendLine("  - 6: Hộp kiểm (Nhiều lựa chọn, ví dụ: dịp sử dụng phù hợp, tính năng bổ sung).");
            sb.AppendLine("- tùy chọn (danh sách các chuỗi, yêu cầu cho loại 2 và 6, chứa các giá trị thông dụng hoặc liên quan).");
            sb.AppendLine();
            sb.AppendLine("Đảm bảo rằng:");
            sb.AppendLine("- Các thuộc tính được viết bằng ngôn ngữ đơn giản, rõ ràng và dễ hiểu cho người dùng.");
            sb.AppendLine("- Các thuộc tính chuyển đổi (ví dụ: câu hỏi Có/Không) được chuyển thành Lựa chọn với các tùy chọn: ['Có', 'Không'].");
            sb.AppendLine("- Các thuộc tính bao gồm các đặc điểm vật lý, ngoại hình, tính năng sử dụng, khả năng tùy chỉnh, tính bền vững, đóng gói, công dụng, và các tính năng bổ sung.");
            sb.AppendLine("- Liệt kê tất cả các thuộc tính có liên quan đến danh mục và mô tả đã cho, đảm bảo độ bao quát toàn diện và chi tiết, bao gồm cả thông tin về quy trình sản xuất, nguồn gốc, và các chứng nhận (nếu có).");
            sb.AppendLine("- Cân nhắc các yếu tố như: độ bền, khả năng bảo trì, an toàn cho người sử dụng, và khả năng tái chế của sản phẩm.");
            sb.AppendLine("- Loại trừ các trường sau:");
            sb.AppendLine("  - Tên");
            sb.AppendLine("  - Mô tả");
            sb.AppendLine("  - Ngân sách tối thiểu");
            sb.AppendLine("  - Ngân sách tối đa");
            sb.AppendLine("  - Thời gian");
            sb.AppendLine("  - Số lượng");
            sb.AppendLine();
            sb.AppendLine("Đầu ra nên là một mảng JSON có cấu trúc tốt, trong đó mỗi thuộc tính cần được mô tả chi tiết với các giá trị khả thi.");

            return sb.ToString();
        }



        private MediaType ParseMediaType(int type)
        {
            return type switch
            {
                0 => MediaType.Text,
                1 => MediaType.Number,
                2 => MediaType.Select,
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
            public Guid Id { get; set; } = Guid.NewGuid();
            public string? Name { get; set; }
            public int Type { get; set; } 
            public List<string>? Options { get; set; }
        }


        public class ModelResponse
        {
            public string? Name { get; set; }
            public MediaType Type { get; set; }
            public List<string>? Options { get; set; }
        }
        private string ExtractJsonContent(string response)
{
    int firstBracket = response.IndexOf('[');
    int lastBracket = response.LastIndexOf(']');

    if (firstBracket == -1 || lastBracket == -1 || lastBracket <= firstBracket)
        return string.Empty;

    return response.Substring(firstBracket, lastBracket - firstBracket + 1);
}

    }
}
