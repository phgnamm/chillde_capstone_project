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
    new EventModel
    {
        NameVi = "Tết Dương lịch", NameEn = "New Year's Day", Date = "01-01", IsLunar = false,
        KeywordsVi = new List<string> { "Tết", "Năm mới", "Ngày lễ", "Chúc mừng", "Gia đình", "Du lịch", "Tiệc tùng", "Mừng năm mới", "Chúc phúc", "Lịch nghỉ" },
        KeywordsEn = new List<string> { "New Year", "Celebration", "Holiday", "Family", "Travel", "Party", "Greetings", "Festivity", "New Year's Eve", "Festive season" }
    },
    new EventModel
    {
        NameVi = "Ngày Thầy thuốc Việt Nam", NameEn = "Vietnamese Doctors' Day", Date = "27-02", IsLunar = false,
        KeywordsVi = new List<string> { "Thầy thuốc", "Y tế", "Bác sĩ", "Chăm sóc sức khỏe", "Y học", "Tôn vinh", "Người thầy thuốc", "Ngày truyền thống", "Cảm ơn", "Chăm sóc" },
        KeywordsEn = new List<string> { "Doctors", "Healthcare", "Medical profession", "Doctors' Day", "Vietnamese doctors", "Medicine", "Appreciation", "Honoring", "Health", "Gratitude" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Phụ nữ", NameEn = "International Women's Day", Date = "08-03", IsLunar = false,
        KeywordsVi = new List<string> { "Phụ nữ", "Quyền phụ nữ", "Bình đẳng giới", "Tôn vinh", "Ngày lễ", "Chúc mừng", "Tự do", "Độc lập", "Người phụ nữ", "Vượt qua" },
        KeywordsEn = new List<string> { "Women", "Gender equality", "Women's rights", "Celebration", "Empowerment", "Equality", "Feminism", "Strength", "Respect", "Inspiration" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Hạnh phúc", NameEn = "International Day of Happiness", Date = "20-03", IsLunar = false,
        KeywordsVi = new List<string> { "Hạnh phúc", "Niềm vui", "Cuộc sống", "Ngày lễ", "Chia sẻ", "Cộng đồng", "Tinh thần", "Phúc lợi", "Lạc quan", "Tươi cười" },
        KeywordsEn = new List<string> { "Happiness", "Joy", "Well-being", "Celebration", "Optimism", "Life", "Community", "Smile", "Mental health", "Positivity" }
    },
    new EventModel
    {
        NameVi = "Giỗ Tổ Hùng Vương", NameEn = "Hung Kings' Commemoration Day", Date = "10-03", IsLunar = true,
        KeywordsVi = new List<string> { "Hùng Vương", "Giỗ tổ", "Lịch sử", "Dân tộc", "Truyền thống", "Tôn vinh", "Giới thiệu văn hóa", "Phong tục", "Lễ hội", "Tổ tiên" },
        KeywordsEn = new List<string> { "Hung Kings", "Commemoration", "Tradition", "History", "Culture", "National identity", "Ancestor worship", "Festival", "Heritage", "Ceremony" }
    },
    new EventModel
    {
        NameVi = "Ngày Giải phóng miền Nam", NameEn = "Reunification Day", Date = "30-04", IsLunar = false,
        KeywordsVi = new List<string> { "Giải phóng", "Miền Nam", "Ngày chiến thắng", "Tự do", "Hòa bình", "Lịch sử", "Dân tộc", "Chung tay", "Ngày lễ", "Tinh thần đoàn kết" },
        KeywordsEn = new List<string> { "Reunification", "Victory", "Freedom", "Independence", "History", "South Vietnam", "Peace", "Unity", "National Day", "Celebration" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Lao động", NameEn = "International Workers' Day", Date = "01-05", IsLunar = false,
        KeywordsVi = new List<string> { "Lao động", "Công nhân", "Ngày lễ", "Tôn vinh", "Công bằng", "Công lý", "Quyền lợi", "Chia sẻ", "Xã hội", "Nỗ lực" },
        KeywordsEn = new List<string> { "Labor", "Workers", "International Day", "Social justice", "Rights", "Fairness", "Dignity", "Celebration", "Workforce", "Unity" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Thiếu nhi", NameEn = "International Children's Day", Date = "01-06", IsLunar = false,
        KeywordsVi = new List<string> { "Trẻ em", "Ngày Quốc tế", "Tổ chức", "Phúc lợi trẻ em", "Bảo vệ trẻ em", "Niềm vui", "Học tập", "Chơi đùa", "Chăm sóc", "Tương lai" },
        KeywordsEn = new List<string> { "Children", "International Day", "Rights of children", "Protection", "Joy", "Education", "Future", "Care", "Play", "Family" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Người cao tuổi", NameEn = "International Day of Older Persons", Date = "01-10", IsLunar = false,
        KeywordsVi = new List<string> { "Người cao tuổi", "Tôn vinh", "Lão hóa", "Chăm sóc", "Phúc lợi", "Lão khoa", "Gia đình", "Tương lai", "Ngày lễ", "Sức khỏe" },
        KeywordsEn = new List<string> { "Older persons", "Elderly", "Celebration", "Aging", "Care", "Respect", "Family", "Social welfare", "Health", "Dignity" }
    },
    new EventModel
    {
        NameVi = "Ngày Giải phóng Thủ đô", NameEn = "Hanoi Liberation Day", Date = "10-10", IsLunar = false,
        KeywordsVi = new List<string> { "Giải phóng", "Thủ đô", "Hà Nội", "Lịch sử", "Ngày chiến thắng", "Tự do", "Chúng ta", "Đoàn kết", "Bảo vệ", "Lịch sử dân tộc" },
        KeywordsEn = new List<string> { "Liberation", "Hanoi", "Victory", "Freedom", "National history", "Reunification", "Victory day", "Patriotism", "Independence", "Unity" }
    },
    new EventModel
    {
        NameVi = "Halloween", NameEn = "Halloween", Date = "31-10", IsLunar = false,
        KeywordsVi = new List<string> { "Halloween", "Ma quái", "Trang trí", "Lễ hội", "Hóa trang", "Sợ hãi", "Trick or treat", "Lễ hội Mỹ", "Chơi đùa", "Đêm hội" },
        KeywordsEn = new List<string> { "Halloween", "Spooky", "Costumes", "Trick or treat", "Decoration", "Festival", "Frightening", "Scary", "Night", "Celebration" }
    },
    new EventModel
    {
        NameVi = "Ngày Nhà giáo Việt Nam", NameEn = "Vietnamese Teachers' Day", Date = "20-11", IsLunar = false,
        KeywordsVi = new List<string> { "Nhà giáo", "Giáo viên", "Tôn vinh", "Ngày lễ", "Cảm ơn", "Truyền cảm hứng", "Học sinh", "Phát triển", "Giáo dục", "Kỷ niệm" },
        KeywordsEn = new List<string> { "Teachers", "Education", "Teaching", "Appreciation", "Honor", "Inspiration", "Gratitude", "Students", "Learning", "Celebration" }
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Nam giới", NameEn = "International Men's Day", Date = "19-11", IsLunar = false,
        KeywordsVi = new List<string> { "Nam giới", "Tôn vinh", "Ngày lễ", "Công bằng", "Giới tính", "Bình đẳng", "Sức khỏe", "Gia đình", "Hạnh phúc", "Phát triển" },
        KeywordsEn = new List<string> { "Men", "Gender equality", "Men's Day", "Celebration", "Health", "Family", "Well-being", "Social justice", "Community", "Respect" }
    },
    new EventModel
    {
        NameVi = "Ngày Quân đội Nhân dân Việt Nam", NameEn = "Vietnam People's Army Day", Date = "22-12", IsLunar = false,
        KeywordsVi = new List<string> { "Quân đội", "Nhân dân", "Ngày lễ", "Tôn vinh", "Anh hùng", "Bảo vệ tổ quốc", "Tinh thần", "Dân tộc", "Quân nhân", "Chiến tranh" },
        KeywordsEn = new List<string> { "Army", "People's Army", "Vietnam", "Soldiers", "Commemoration", "Victory", "Nation", "Defense", "Heroes", "Military" }
    },
    new EventModel
    {
        NameVi = "Ngày Giáng sinh (Noel)", NameEn = "Christmas Eve", Date = "24-12", IsLunar = false,
        KeywordsVi = new List<string> { "Giáng sinh", "Noel", "Lễ hội", "Tôn vinh", "Tình yêu", "Gia đình", "Mừng Chúa Giáng sinh", "Món quà", "Lễ hội mùa đông", "Tổ chức" },
        KeywordsEn = new List<string> { "Christmas", "Eve", "Holiday", "Celebration", "Family", "Love", "Gifts", "Religion", "Winter festival", "Togetherness" }
    },
    new EventModel
    {
        NameVi = "Tết Nguyên Đán", NameEn = "Lunar New Year", Date = "01-01", IsLunar = true,
        KeywordsVi = new List<string> { "Tết", "Lịch Nguyên Đán", "Tết cổ truyền", "Mừng năm mới", "Lễ hội", "Gia đình", "Tặng quà", "Chúc mừng", "Tượng trưng", "Phong tục" },
        KeywordsEn = new List<string> { "Lunar New Year", "Tet", "Traditional", "Festivity", "New Year", "Celebration", "Family", "Culture", "Tradition", "Gifts" }
    },
    new EventModel
    {
        NameVi = "Lễ Vu Lan", NameEn = "Vu Lan Festival", Date = "15-07", IsLunar = true,
        KeywordsVi = new List<string> { "Vu Lan", "Báo hiếu", "Tôn vinh", "Lễ hội", "Cúng tổ tiên", "Phúc đức", "Tình mẫu tử", "Hiếu hạnh", "Cộng đồng", "Tâm linh" },
        KeywordsEn = new List<string> { "Vu Lan", "Ancestral worship", "Filial piety", "Mother's love", "Festival", "Respect", "Tradition", "Honor", "Spiritual", "Cultural celebration" }
    }
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
            return new { EventVi = e.NameVi, EventEn = e.NameEn, Date = eventDate, KeywordsVi = e.KeywordsVi, KeywordsEn = e.KeywordsEn };
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
                    Data = new { pastRecentEvent.EventVi, pastRecentEvent.EventEn, Date = pastRecentEvent.Date.ToString("yyyy-MM-dd"), KeywordsVi = pastRecentEvent.KeywordsVi, KeywordsEn = pastRecentEvent.KeywordsEn }
                };
            }
            if (nearestEvent != null && (nearestEvent.Date - currentDate).TotalDays <= maxDaysThreshold)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = sourLanguageCode == "vi" ? "Lấy sự kiện thành công" : "Get Event Successfully",
                    Data = new { nearestEvent.EventVi, nearestEvent.EventEn, Date = nearestEvent.Date.ToString("yyyy-MM-dd"), KeywordsVi = nearestEvent.KeywordsVi, KeywordsEn = nearestEvent.KeywordsEn }
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
