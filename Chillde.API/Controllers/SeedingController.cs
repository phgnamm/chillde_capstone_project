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
                            MinWeight = 1000,
                            MaxWeight = 10000,
                            CategoryId = Guid.Parse("85a007bc-b215-46fe-98bc-eac1d44e2234"),
                            ServiceAttachments = new List<ServiceAttachmentAddModel>
                            {
                                new ServiceAttachmentAddModel
                                {
                                    AttachmentUrl = CreateFormFileFromPath("Images\\Screenshot 2025-03-19 213306.png"),
                                    AttachmentAlt = "vida"
                                }
                            }
                        },       
                    // Dịch vụ 2: Khuyên Tai Đính Đá (Danh mục: Earrings - Id: 3f8c0d2a-4b56-4ffb-f68b-2f5f6a7b9012)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Khuyên Tai Đính Đá",
    //                        Description = "Mang đến vẻ đẹp lấp lánh với những đôi khuyên tai đính đá được chế tác tinh xảo. Sản phẩm của chúng tôi kết hợp giữa thiết kế hiện đại và chất liệu cao cấp, phù hợp cho mọi dịp từ thường ngày đến sự kiện đặc biệt.\r\n\r\nĐặc Điểm:\r\nChất Liệu Cao Cấp: Sử dụng bạc 925 và đá quý tự nhiên.\r\nThiết Kế Độc Đáo: Mỗi mẫu đều được thiết kế để tôn lên vẻ đẹp của người đeo.\r\nĐộ Bền Cao: Chống xỉn màu và giữ được độ sáng bóng lâu dài.\r\nTùy Chỉnh: Có thể thêm khắc tên hoặc ký tự cá nhân hóa.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm thủ công tỉ mỉ, đảm bảo chất lượng vượt trội.\r\nDịch vụ hỗ trợ tận tâm, giao hàng nhanh chóng.\r\nHãy để đôi khuyên tai này trở thành điểm nhấn hoàn hảo cho phong cách của bạn!",
    //                        MinWeight = 1000,
    //                        MaxWeight = 5000,
    //                        CategoryId = Guid.Parse("3f8c0d2a-4b56-4ffb-f68b-2f5f6a7b9012"), // Earrings
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Khuyên tai đính đá lấp lánh"
    //                            }
    //                        }
    //                    },
    //                    // Dịch vụ 3: Dây Chuyền Cá Nhân Hóa (Danh mục: Necklaces - Id: 2c7f9a1d-3e45-46ac-c57e-1c4c5d6e8901)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Dây Chuyền Cá Nhân Hóa",
    //                        Description = "Tạo dấu ấn riêng với dây chuyền cá nhân hóa được làm từ vàng hoặc bạc chất lượng cao. Đây là món quà hoàn hảo để lưu giữ những kỷ niệm đặc biệt.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Vàng 18K hoặc bạc 925 tùy chọn.\r\nKhắc Tên: Khắc tên, ngày kỷ niệm hoặc thông điệp theo yêu cầu.\r\nThiết Kế Tinh Tế: Phù hợp với mọi độ tuổi và phong cách.\r\nĐóng Gói Sang Trọng: Hộp quà cao cấp kèm theo.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác thủ công với sự chú ý đến từng chi tiết.\r\nDịch vụ tư vấn tận tình, giao hàng an toàn.\r\nHãy biến ý tưởng của bạn thành hiện thực với dây chuyền độc nhất này!",
    //                        MinWeight = 200,
    //                        MaxWeight = 1500,
    //                        CategoryId = Guid.Parse("2c7f9a1d-3e45-46ac-c57e-1c4c5d6e8901"), // Necklaces
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Dây chuyền khắc tên cá nhân hóa"
    //                            }
    //                        }
    //                    },
    //                    // Dịch vụ 4: Nhẫn Đính Hôn (Danh mục: Rings - Id: 3b8e0f2c-4d56-4bfb-b68d-2b5b6c7d9012)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Nhẫn Đính Hôn",
    //                        Description = "Biểu tượng của tình yêu vĩnh cửu với nhẫn đính hôn được thiết kế riêng theo câu chuyện của bạn. Chúng tôi sử dụng kim cương tự nhiên và vàng cao cấp để tạo nên những chiếc nhẫn hoàn hảo.\r\n\r\nĐặc Điểm:\r\nKim Cương Chất Lượng: Đạt tiêu chuẩn GIA, đảm bảo độ trong và sáng.\r\nChất Liệu: Vàng trắng 14K/18K hoặc bạch kim.\r\nTùy Chỉnh: Chọn kích thước kim cương và kiểu dáng.\r\nBảo Hành: Bảo hành trọn đời cho sản phẩm.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác bởi các chuyên gia trang sức hàng đầu.\r\nCam kết chất lượng và sự hài lòng tuyệt đối.\r\nHãy để chúng tôi giúp bạn nói lời cầu hôn theo cách đặc biệt nhất!",
    //                        MinWeight = 3,
    //                        MaxWeight = 20,
    //                        CategoryId = Guid.Parse("3b8e0f2c-4d56-4bfb-b68d-2b5b6c7d9012"), // Rings
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Nhẫn đính hôn kim cương"
    //                            }
    //                        }
    //                    },
    //                    // Dịch vụ 5: Đồng Hồ Cổ Điển (Danh mục: Watches - Id: 3f8c0d2a-4b56-41df-f68b-2f5f6a7b9012)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Đồng Hồ Cổ Điển",
    //                        Description = "Khám phá bộ sưu tập đồng hồ cổ điển mang phong cách vượt thời gian. Mỗi chiếc đồng hồ đều được chế tác với sự tỉ mỉ và mang đậm dấu ấn nghệ thuật.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Thép không gỉ và da thật cao cấp.\r\nMáy Cơ: Sử dụng cơ chế lên dây cót thủ công hoặc tự động.\r\nThiết Kế: Mặt số cổ điển, kim và số La Mã tinh tế.\r\nBảo Hành: 2 năm cho máy móc và vỏ.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm độc đáo, phù hợp với những người yêu thích phong cách vintage.\r\nDịch vụ bảo trì và sửa chữa chuyên nghiệp.\r\nHãy sở hữu một chiếc đồng hồ không chỉ để xem giờ mà còn là một tác phẩm nghệ thuật!",
    //                        MinWeight = 50,
    //                        MaxWeight = 200,
    //                        CategoryId = Guid.Parse("3f8c0d2a-4b56-41df-f68b-2f5f6a7b9012"), // Watches
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Đồng hồ cổ điển phong cách vintage"
    //                            }
    //                        }
    //                    },
    //                    // Dịch vụ 6: Kẹp Tiền Kim Loại (Danh mục: Money Clips - Id: 3e8a7b5c-9d01-4f4a-a23b-7c0d1e2f4567)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Kẹp Tiền Kim Loại",
    //                        Description = "Tăng thêm phong cách với kẹp tiền kim loại được chế tác tinh xảo. Sản phẩm nhỏ gọn nhưng tiện dụng, phù hợp cho những ai yêu thích sự tối giản.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Thép không gỉ hoặc hợp kim cao cấp.\r\nThiết Kế: Nhỏ gọn, dễ dàng mang theo trong túi.\r\nKhắc Cá Nhân: Tùy chọn khắc tên hoặc biểu tượng.\r\nĐộ Bền: Chống gỉ sét và giữ độ sáng lâu dài.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được làm thủ công với độ chính xác cao.\r\nDịch vụ giao hàng nhanh, hỗ trợ tận tình.\r\nHãy để kẹp tiền này thay thế ví truyền thống của bạn một cách phong cách!",
    //                        MinWeight = 1000,
    //                        MaxWeight = 5000,
    //                        CategoryId = Guid.Parse("3e8a7b5c-9d01-4f4a-a23b-7c0d1e2f4567"), // Money Clips
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Kẹp tiền kim loại sang trọng"
    //                            }
    //                        }
    //                    },
    //                                        // Dịch vụ 7: Vòng Tay Hạt Đá (Danh mục: Beaded Bracelets - Id: 8c3f5a7d-9e01-4fce-c13e-7c0c1d2e4567)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Vòng Tay Hạt Đá",
    //                        Description = "Khám phá vẻ đẹp tự nhiên với vòng tay hạt đá được làm từ các loại đá quý phong thủy. Mỗi chiếc vòng đều mang năng lượng tích cực và ý nghĩa riêng.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Đá tự nhiên (thạch anh, ngọc bích, mã não...).\r\nThiết Kế: Hạt tròn đều, kết hợp dây đan thủ công.\r\nTùy Chỉnh: Chọn loại đá và kích thước phù hợp.\r\nÝ Nghĩa: Mang lại may mắn, bình an cho người đeo.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác thủ công với sự chăm chút.\r\nĐảm bảo đá thật 100%, kiểm định chất lượng.\r\nHãy chọn chiếc vòng tay hoàn hảo để đồng hành cùng bạn!",
    //                        MinWeight = 1000,
    //                        MaxWeight = 5000,
    //                        CategoryId = Guid.Parse("8c3f5a7d-9e01-4fce-c13e-7c0c1d2e4567"), // Beaded Bracelets
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Vòng tay hạt đá phong thủy"
    //                            }
    //                        }
    //                    },

    //                    // Dịch vụ 8: Gài Áo Thời Trang (Danh mục: Brooches - Id: 4c9f1a3d-5e67-48ce-c79e-3c6c7d8e0123)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Gài Áo Thời Trang",
    //                        Description = "Tô điểm trang phục của bạn với gài áo thời trang được thiết kế độc đáo. Sản phẩm là sự kết hợp giữa nghệ thuật và phong cách hiện đại.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Hợp kim mạ vàng/bạc và đá trang trí.\r\nThiết Kế: Hình hoa, động vật hoặc họa tiết trừu tượng.\r\nỨng Dụng: Phù hợp với áo vest, khăn choàng hoặc váy.\r\nĐộ Bền: Giữ màu tốt, không gây kích ứng da.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm thủ công, mang tính thẩm mỹ cao.\r\nDịch vụ tư vấn chọn mẫu phù hợp với phong cách.\r\nThêm điểm nhấn tinh tế cho outfit của bạn ngay hôm nay!",
    //                        MinWeight = 500,
    //                        MaxWeight = 2000,
    //                        CategoryId = Guid.Parse("4c9f1a3d-5e67-48ce-c79e-3c6c7d8e0123"), // Brooches
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Gài áo thời trang tinh xảo"
    //                            }
    //                        }
    //                    },

    //                    new ServiceAddModel
    //                    {
    //                        Name = "Hộp Đựng Trang Sức Gỗ",
    //                        Description = "Bảo quản trang sức của bạn với hộp đựng làm từ gỗ tự nhiên, thiết kế sang trọng và tiện lợi. Đây là lựa chọn hoàn hảo để giữ gìn những món đồ quý giá.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Gỗ óc chó hoặc gỗ sồi cao cấp.\r\nThiết Kế: Nhiều ngăn, lớp lót nhung mềm mại.\r\nTùy Chỉnh: Khắc tên hoặc hoa văn theo yêu cầu.\r\nĐộ Bền: Chống ẩm, chống trầy xước.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được làm thủ công, đảm bảo chất lượng.\r\nDịch vụ giao hàng an toàn, hỗ trợ sau mua.\r\nHãy để trang sức của bạn luôn an toàn và đẹp mắt!",
    //                        MinWeight = 200,
    //                        MaxWeight = 1000,
    //                        CategoryId = Guid.Parse("9f4c6d8a-0b12-49df-f24b-8f1f2a3b5678"), // Jewelry Boxes
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = null,
    //                                AttachmentAlt = "Hộp đựng trang sức gỗ sang trọng"
    //                            }
    //                        }
    //},


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
        private static IFormFile CreateFormFileFromPath(string relativePath)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(baseDirectory, relativePath.TrimStart('\\', '/'));

            if (!System.IO.File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File không tồn tại: {fullPath}");
            }

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, "AttachmentUrl", Path.GetFileName(fullPath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

    }

}
