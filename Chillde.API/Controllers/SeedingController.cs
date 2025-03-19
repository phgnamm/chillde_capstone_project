using Chillde.Services.Interfaces;
using Chillde.Services.Models.ServiceAttachmentModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/seeding")]
    [ApiController]
    public class SeedingController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public SeedingController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpPost("seed-services")]
        public async Task<IActionResult> SeedServices()
        {
            try
            {
                var sampleServices = new List<ServiceAddModel>
                {
                    new ServiceAddModel
                    {
                        Name = "Ví Da Thủ Công",
                        Description = "Chúng tôi tự hào mang đến cho bạn những chiếc ví da handmade độc đáo và tinh tế, được thiết kế và chế tạo bởi những nghệ nhân có tay nghề cao. Mỗi chiếc ví đều là một tác phẩm nghệ thuật, được tạo ra từ những tấm da chất lượng tốt nhất và được chăm chút từng chi tiết.\r\n\r\nĐặc Điểm Của Dịch Vụ Của Chúng Tôi:\r\nChất Liệu Da Tinh Khiết: Sử dụng da thật 100%, đảm bảo độ bền và sự sang trọng.\r\nThiết Kế Tùy Biến: Khách hàng có thể lựa chọn màu sắc, kiểu dáng và kích thước theo sở thích cá nhân.\r\nChế Tác Tay Nghề: Mỗi chiếc ví đều được làm thủ công bởi những người thợ có kinh nghiệm lâu năm.\r\nChất Lượng Đảm Bảo: Kiểm tra kỹ lưỡng từng sản phẩm trước khi giao hàng.\r\nDịch Vụ Tối Ưu: Hỗ trợ khách hàng trong suốt quá trình từ tư vấn đến sau mua hàng.\r\n\r\nTại Sao Nên Chọn Dịch Vụ Của Chúng Tôi?\r\nSản Phẩm Độc Đáo: Mỗi chiếc ví là một tác phẩm nghệ thuật độc nhất.\r\nChất Lượng Tuyệt Đối: Đảm bảo sự hài lòng và tin tưởng của khách hàng.\r\nDịch Vụ Cá Nhân Hóa: Phù hợp với phong cách và nhu cầu riêng của từng khách hàng.\r\n\r\nHãy để chúng tôi giúp bạn sở hữu một chiếc ví da handmade không chỉ là phụ kiện thời trang mà còn là một món quà ý nghĩa cho bản thân hoặc người thân yêu. Liên hệ với chúng tôi ngay hôm nay để được tư vấn và đặt hàng!",
                        MinWeight = 1,
                        MaxWeight = 10,
                        CategoryId = Guid.Parse("85a007bc-b215-46fe-98bc-eac1d44e2234"),
                        ServiceAttachments = new List<ServiceAttachmentAddModel>
                        {
                            new ServiceAttachmentAddModel
                            {
                                AttachmentUrl = CreateFormFileFromPath("C:\\Users\\tranc\\Pictures\\Screenshots\\Screenshot 2025-03-13 103720.png"),
                                AttachmentAlt = "vida"
                            }
                        }
                    },
                };

                string sourceLanguageCode = "vi";
                string targetLanguageCode = "en";

                foreach (var service in sampleServices)
                {
                    var result = await _serviceService.AddAsync(service, sourceLanguageCode, targetLanguageCode);
                    if (result.Code != StatusCodes.Status201Created)
                    {
                        return StatusCode(result.Code, result);
                    }
                }

                return Ok(new { Message = "Seeding successfully"});
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = ex.Message
                });
            }
        }
        private static IFormFile CreateFormFileFromPath(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File không tồn tại: {filePath}");
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, "AttachmentUrl", Path.GetFileName(filePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" 
            };
        }
    }

}
