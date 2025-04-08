using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ServiceAttachmentModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/seeding")]
    [ApiController]
    public class SeedingController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IPackageService _packageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeatureService _featureService;

        public SeedingController(IServiceService serviceService, IPackageService packageService, IUnitOfWork unitOfWork, IFeatureService featureService)
        {
            _serviceService = serviceService;
            _packageService = packageService;
            _unitOfWork = unitOfWork;
            _featureService = featureService;
        }
        [Authorize]
        [HttpPost("seed-services")]
        public async Task<IActionResult> SeedServices()
        {
            try
            {
                var sampleServices = new List<ServiceAddModel>
                {
                        //new ServiceAddModel
                        //{
                        //    Name = "Ví Da Thủ Công",
                        //    Description = "Chúng tôi tự hào mang đến cho bạn những chiếc ví da handmade độc đáo và tinh tế, được thiết kế và chế tạo bởi những nghệ nhân có tay nghề cao. Mỗi chiếc ví đều là một tác phẩm nghệ thuật, được tạo ra từ những tấm da chất lượng tốt nhất và được chăm chút từng chi tiết.\r\n\r\nĐặc Điểm Của Dịch Vụ Của Chúng Tôi:\r\nChất Liệu Da Tinh Khiết: Sử dụng da thật 100%, đảm bảo độ bền và sự sang trọng.\r\nThiết Kế Tùy Biến: Khách hàng có thể lựa chọn màu sắc, kiểu dáng và kích thước theo sở thích cá nhân.\r\nChế Tác Tay Nghề: Mỗi chiếc ví đều được làm thủ công bởi những người thợ có kinh nghiệm lâu năm.\r\nChất Lượng Đảm Bảo: Kiểm tra kỹ lưỡng từng sản phẩm trước khi giao hàng.\r\nDịch Vụ Tối Ưu: Hỗ trợ khách hàng trong suốt quá trình từ tư vấn đến sau mua hàng.\r\n\r\nTại Sao Nên Chọn Dịch Vụ Của Chúng Tôi?\r\nSản Phẩm Độc Đáo: Mỗi chiếc ví là một tác phẩm nghệ thuật độc nhất.\r\nChất Lượng Tuyệt Đối: Đảm bảo sự hài lòng và tin tưởng của khách hàng.\r\nDịch Vụ Cá Nhân Hóa: Phù hợp với phong cách và nhu cầu riêng của từng khách hàng.\r\n\r\nHãy để chúng tôi giúp bạn sở hữu một chiếc ví da handmade không chỉ là phụ kiện thời trang mà còn là một món quà ý nghĩa cho bản thân hoặc người thân yêu. Liên hệ với chúng tôi ngay hôm nay để được tư vấn và đặt hàng!",
                        //    MinWeight = 1000,
                        //    MaxWeight = 10000,
                        //    CategoryId = Guid.Parse("0b5d4e3f-6a78-4c1d-d90e-4f7a8b9c1234"),
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("ethan-rougon-oIlix2slmsI-unsplash.jpg"),
                        //            AttachmentAlt = "vida"
                        //        }
                        //    }
                        //},       
                    // Dịch vụ 2: Khuyên Tai Đính Đá (Danh mục: Earrings - Id: 3f8c0d2a-4b56-4ffb-f68b-2f5f6a7b9012)
    //                    new ServiceAddModel
    //                    {

    //                        Name = "Khuyên Tai Đính Đá",
    //                        Description = "Mang đến vẻ đẹp lấp lánh với những đôi khuyên tai đính đá được chế tác tinh xảo. Sản phẩm của chúng tôi kết hợp giữa thiết kế hiện đại và chất liệu cao cấp, phù hợp cho mọi dịp từ thường ngày đến sự kiện đặc biệt.\r\n\r\nĐặc Điểm:\r\nChất Liệu Cao Cấp: Sử dụng bạc 925 và đá quý tự nhiên.\r\nThiết Kế Độc Đáo: Mỗi mẫu đều được thiết kế để tôn lên vẻ đẹp của người đeo.\r\nĐộ Bền Cao: Chống xỉn màu và giữ được độ sáng bóng lâu dài.\r\nTùy Chỉnh: Có thể thêm khắc tên hoặc ký tự cá nhân hóa.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm thủ công tỉ mỉ, đảm bảo chất lượng vượt trội.\r\nDịch vụ hỗ trợ tận tâm, giao hàng nhanh chóng.\r\nHãy để đôi khuyên tai này trở thành điểm nhấn hoàn hảo cho phong cách của bạn!",
    //                        MinWeight = 100,
    //                        MaxWeight = 2000,
    //                        CategoryId = Guid.Parse("ba815129-7ad6-4d2d-ba7b-a43977bc310e"), // Earrings
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("simona-sergi-WNn5xbWfkLI-unsplash.jpg"),
    //                                AttachmentAlt = "Khuyên tai đính đá lấp lánh"
    //                            }
    //                        }
    //                    },
    ////                    // Dịch vụ 3: Dây Chuyền Cá Nhân Hóa (Danh mục: Necklaces - Id: 2c7f9a1d-3e45-46ac-c57e-1c4c5d6e8901)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Dây Chuyền Cá Nhân Hóa",
    //                        Description = "Tạo dấu ấn riêng với dây chuyền cá nhân hóa được làm bạc chất lượng cao. Đây là món quà hoàn hảo để lưu giữ những kỷ niệm đặc biệt.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Vàng 18K hoặc bạc 925 tùy chọn.\r\nKhắc Tên: Khắc tên, ngày kỷ niệm hoặc thông điệp theo yêu cầu.\r\nThiết Kế Tinh Tế: Phù hợp với mọi độ tuổi và phong cách.\r\nĐóng Gói Sang Trọng: Hộp quà cao cấp kèm theo.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác thủ công với sự chú ý đến từng chi tiết.\r\nDịch vụ tư vấn tận tình, giao hàng an toàn.\r\nHãy biến ý tưởng của bạn thành hiện thực với dây chuyền độc nhất này!",
    //                        MinWeight = 200,
    //                        MaxWeight = 1500,
    //                        CategoryId = Guid.Parse("2c7f9a1d-3e45-46ac-c57e-1c4c5d6e8901"), // Necklaces
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("gabrielle-henderson-Z0KoI2aysro-unsplash.jpg"),
    //                                AttachmentAlt = "Dây chuyền khắc tên cá nhân hóa"
    //                            }
    //                        }
    //                    },
    ////                    // Dịch vụ 4: Nhẫn Đính Hôn (Danh mục: Rings - Id: 3b8e0f2c-4d56-4bfb-b68d-2b5b6c7d9012)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Nhẫn Đính Hôn",
    //                        Description = "Biểu tượng của tình yêu vĩnh cửu với nhẫn đính hôn được thiết kế riêng theo câu chuyện của bạn. Chúng tôi sử dụng kim cương tự nhiên và vàng cao cấp để tạo nên những chiếc nhẫn hoàn hảo.\r\n\r\nĐặc Điểm:\r\nKim Cương Chất Lượng: Đạt tiêu chuẩn GIA, đảm bảo độ trong và sáng.\r\nChất Liệu: Vàng trắng 14K/18K hoặc bạch kim.\r\nTùy Chỉnh: Chọn kích thước kim cương và kiểu dáng.\r\nBảo Hành: Bảo hành trọn đời cho sản phẩm.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác bởi các chuyên gia trang sức hàng đầu.\r\nCam kết chất lượng và sự hài lòng tuyệt đối.\r\nHãy để chúng tôi giúp bạn nói lời cầu hôn theo cách đặc biệt nhất!",
    //                        MinWeight = 3,
    //                        MaxWeight = 20,
    //                        CategoryId = Guid.Parse("6e1b3c5f-7a89-48ce-e91a-5e8e9f0a2345"), // Rings
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("luigi-pozzoli-jZrfY30y6Kc-unsplash.jpg"),
    //                                AttachmentAlt = "Nhẫn đính hôn kim cương"
    //                            }
    //                        }
    //                    },
    ////                    // Dịch vụ 5: Đồng Hồ Cổ Điển (Danh mục: Watches - Id: 3f8c0d2a-4b56-41df-f68b-2f5f6a7b9012)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Đồng Hồ Cổ Điển",
    //                        Description = "Khám phá bộ sưu tập đồng hồ cổ điển mang phong cách vượt thời gian. Mỗi chiếc đồng hồ đều được chế tác với sự tỉ mỉ và mang đậm dấu ấn nghệ thuật.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Thép không gỉ và da thật cao cấp.\r\nMáy Cơ: Sử dụng cơ chế lên dây cót thủ công hoặc tự động.\r\nThiết Kế: Mặt số cổ điển, kim và số La Mã tinh tế.\r\nBảo Hành: 2 năm cho máy móc và vỏ.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm độc đáo, phù hợp với những người yêu thích phong cách vintage.\r\nDịch vụ bảo trì và sửa chữa chuyên nghiệp.\r\nHãy sở hữu một chiếc đồng hồ không chỉ để xem giờ mà còn là một tác phẩm nghệ thuật!",
    //                        MinWeight = 50,
    //                        MaxWeight = 200,
    //                        CategoryId = Guid.Parse("5b0e2f4c-6d78-4ffb-b80d-4b7b8c9d1234"), // Watches
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("vvs--KRN2kU9e1s-unsplash.jpg"),
    //                                AttachmentAlt = "Đồng hồ cổ điển phong cách vintage"
    //                            }
    //                        }
    //                    },
    ////                    // Dịch vụ 6: Kẹp Tiền Kim Loại (Danh mục: Money Clips - Id: 3e8a7b5c-9d01-4f4a-a23b-7c0d1e2f4567)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Kẹp Tiền Kim Loại",
    //                        Description = "Tăng thêm phong cách với kẹp tiền kim loại được chế tác tinh xảo. Sản phẩm nhỏ gọn nhưng tiện dụng, phù hợp cho những ai yêu thích sự tối giản.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Thép không gỉ hoặc hợp kim cao cấp.\r\nThiết Kế: Nhỏ gọn, dễ dàng mang theo trong túi.\r\nKhắc Cá Nhân: Tùy chọn khắc tên hoặc biểu tượng.\r\nĐộ Bền: Chống gỉ sét và giữ độ sáng lâu dài.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được làm thủ công với độ chính xác cao.\r\nDịch vụ giao hàng nhanh, hỗ trợ tận tình.\r\nHãy để kẹp tiền này thay thế ví truyền thống của bạn một cách phong cách!",
    //                        MinWeight = 100,
    //                        MaxWeight = 500,
    //                        CategoryId = Guid.Parse("8f3b2c1d-4e56-4a9b-b78c-2d5e6f7a9012"), // Money Clips
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("mayank-gaur-aXlkSSPakJk-unsplash.jpg"),
    //                                AttachmentAlt = "Kẹp tiền kim loại sang trọng"
    //                            }
    //                        }
    //                    },
    //               // Dịch vụ 7: Vòng Tay Hạt Đá (Danh mục: Beaded Bracelets - Id: 8c3f5a7d-9e01-4fce-c13e-7c0c1d2e4567)
    //                    new ServiceAddModel
    //                    {
    //                        Name = "Vòng Tay Hạt Đá",
    //                        Description = "Khám phá vẻ đẹp tự nhiên với vòng tay hạt đá được làm từ các loại đá quý phong thủy. Mỗi chiếc vòng đều mang năng lượng tích cực và ý nghĩa riêng.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Đá tự nhiên (thạch anh, ngọc bích, mã não...).\r\nThiết Kế: Hạt tròn đều, kết hợp dây đan thủ công.\r\nTùy Chỉnh: Chọn loại đá và kích thước phù hợp.\r\nÝ Nghĩa: Mang lại may mắn, bình an cho người đeo.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được chế tác thủ công với sự chăm chút.\r\nĐảm bảo đá thật 100%, kiểm định chất lượng.\r\nHãy chọn chiếc vòng tay hoàn hảo để đồng hành cùng bạn!",
    //                        MinWeight = 100,
    //                        MaxWeight = 1000,
    //                        CategoryId = Guid.Parse("8c3f5a7d-9e01-4fce-c13e-7c0c1d2e4567"), // Beaded Bracelets
    //                        ServiceAttachments = new List<ServiceAttachmentAddModel>
    //                        {
    //                            new ServiceAttachmentAddModel
    //                            {
    //                                AttachmentUrl = CreateFormFileFromPath("freestocks-ziiUkV9LpdU-unsplash.jpg"),
    //                                AttachmentAlt = "Vòng tay hạt đá phong thủy"
    //                            }
    //                        }
    //                    },

    //                    // Dịch vụ 8: Gài Áo Thời Trang (Danh mục: Brooches - Id: 4c9f1a3d-5e67-48ce-c79e-3c6c7d8e0123)
                        //new ServiceAddModel
                        //{
                        //    Name = "Gài Áo Thời Trang",
                        //    Description = "Tô điểm trang phục của bạn với gài áo thời trang được thiết kế độc đáo. Sản phẩm là sự kết hợp giữa nghệ thuật và phong cách hiện đại.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Hợp kim mạ vàng/bạc và đá trang trí.\r\nThiết Kế: Hình hoa, động vật hoặc họa tiết trừu tượng.\r\nỨng Dụng: Phù hợp với áo vest, khăn choàng hoặc váy.\r\nĐộ Bền: Giữ màu tốt, không gây kích ứng da.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm thủ công, mang tính thẩm mỹ cao.\r\nDịch vụ tư vấn chọn mẫu phù hợp với phong cách.\r\nThêm điểm nhấn tinh tế cho outfit của bạn ngay hôm nay!",
                        //    MinWeight = 500,
                        //    MaxWeight = 2000,
                        //    CategoryId = Guid.Parse("4c9f1a3d-5e67-48ce-c79e-3c6c7d8e0123"), // Brooches
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("parisa-safaei-H8c0XcyN_PA-unsplash.jpg"),
                        //            AttachmentAlt = "Gài áo thời trang tinh xảo"
                        //        }
                        //    }
                        //},

                        //new ServiceAddModel
                        //{
                        //    Name = "Hộp Đựng Trang Sức Gỗ",
                        //    Description = "Bảo quản trang sức của bạn với hộp đựng làm từ gỗ tự nhiên, thiết kế sang trọng và tiện lợi. Đây là lựa chọn hoàn hảo để giữ gìn những món đồ quý giá.\r\n\r\nĐặc Điểm:\r\nChất Liệu: Gỗ óc chó hoặc gỗ sồi cao cấp.\r\nThiết Kế: Nhiều ngăn, lớp lót nhung mềm mại.\r\nTùy Chỉnh: Khắc tên hoặc hoa văn theo yêu cầu.\r\nĐộ Bền: Chống ẩm, chống trầy xước.\r\n\r\nLý Do Chọn Chúng Tôi:\r\nSản phẩm được làm thủ công, đảm bảo chất lượng.\r\nDịch vụ giao hàng an toàn, hỗ trợ sau mua.\r\nHãy để trang sức của bạn luôn an toàn và đẹp mắt!",
                        //    MinWeight = 200,
                        //    MaxWeight = 1000,
                        //    CategoryId = Guid.Parse("9f4c6d8a-0b12-49df-f24b-8f1f2a3b5678"), // Jewelry Boxes
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("clem-onojeghuo-HpK0nVi7xOw-unsplash.jpg"),
                        //            AttachmentAlt = "Hộp đựng trang sức gỗ sang trọng"
                        //        }
                        //    }
                        //},
                        //// Dịch vụ 10: Chain Wallet (Danh mục: Chain Wallets)
                        //new ServiceAddModel
                        //{
                        //    Name = "Ví Dây Xích Thời Trang",
                        //    Description = "Thể hiện phong cách cá tính với ví chuỗi thời trang đẳng cấp. Được làm từ chất liệu da thật, kết hợp với dây xích kim loại tạo điểm nhấn mạnh mẽ.\r\n\r\nĐặc Điểm:\r\n- Chất Liệu: Da thật, hợp kim chống gỉ.\r\n- Thiết Kế: Kiểu dáng sang trọng, kết hợp dây xích tháo rời.\r\n- Tính Ứng Dụng: Phù hợp cho cả nam và nữ, tiện lợi mang theo khi đi chơi, đi làm.\r\n\r\n **Gợi ý dịp tặng:** Phù hợp làm quà tặng cho bạn trai, người yêu vào các dịp sinh nhật, Giáng sinh, hoặc Tết Dương lịch.",
                        //    MinWeight = 200,
                        //    MaxWeight = 1000,
                        //    CategoryId = Guid.Parse("0b5d4e3f-6a78-4c1d-d90e-4f7a8b9c1234"), // Chain Wallets
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("chain_wallet.jpg"),
                        //            AttachmentAlt = "Ví chuỗi thời trang phong cách"
                        //        }
                        //    }
                        //},

                        //// Dịch vụ 11: Ring Trees (Danh mục: Ring Trees)
                        //new ServiceAddModel
                        //{
                        //    Name = "Giá Đỡ Nhẫn Nghệ Thuật",
                        //    Description = "Trang trí không gian của bạn với giá đỡ nhẫn độc đáo, giúp bảo quản nhẫn ngăn nắp và thẩm mỹ.\r\n\r\nĐặc Điểm:\r\n- Chất Liệu: Gỗ tự nhiên hoặc hợp kim cao cấp.\r\n- Thiết Kế: Kiểu dáng cây nghệ thuật, giúp trưng bày nhiều nhẫn cùng lúc.\r\n- Ứng Dụng: Thích hợp cho bàn trang điểm hoặc cửa hàng trang sức.\r\n\r\n **Gợi ý dịp tặng:** Quà tặng hoàn hảo cho những ai yêu thích trang sức, đặc biệt là vào Ngày Quốc tế Phụ nữ hoặc Giáng sinh.",
                        //    MinWeight = 300,
                        //    MaxWeight = 1200,
                        //    CategoryId = Guid.Parse("1b6e8f0c-2d34-47fb-b46d-0b3b4c5d7890"), // Ring Trees
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("ring_tree.jpg"),
                        //            AttachmentAlt = "Giá đỡ nhẫn nghệ thuật"
                        //        }
                        //    }
                        //},

                        //// Dịch vụ 12: Arm Bands (Danh mục: Arm Bands)
                        //new ServiceAddModel
                        //{
                        //    Name = "Vòng Tay Bắp Tay Cá Tính",
                        //    Description = "Tôn lên vẻ đẹp cá tính với vòng tay bắp tay phong cách boho hoặc hiện đại. Một phụ kiện độc đáo dành cho những người yêu thời trang.\r\n\r\nĐặc Điểm:\r\n- Chất Liệu: Hợp kim cao cấp, mạ vàng/bạc.\r\n- Thiết Kế: Kiểu dáng tinh tế, có thể điều chỉnh kích thước.\r\n- Phù Hợp: Thích hợp cho những buổi tiệc, chụp ảnh hoặc lễ hội.\r\n\r\n **Gợi ý dịp tặng:** Lựa chọn lý tưởng cho Ngày Quốc tế Phụ nữ hoặc Ngày Quốc tế Hạnh phúc.",
                        //    MinWeight = 150,
                        //    MaxWeight = 800,
                        //    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("arm_band.jpg"),
                        //            AttachmentAlt = "Vòng tay bắp tay sang trọng"
                        //        }
                        //    }
                        //},

                        //// Dịch vụ 13: Belt Buckles (Danh mục: Belt Buckles)
                        //new ServiceAddModel
                        //{
                        //    Name = "Mặt Khóa Thắt Lưng Độc Đáo",
                        //    Description = "Nâng tầm phong cách với mặt khóa thắt lưng thiết kế sang trọng. Một phụ kiện không thể thiếu để hoàn thiện set đồ của bạn.\r\n\r\nĐặc Điểm:\r\n- Chất Liệu: Hợp kim cao cấp, chống gỉ.\r\n- Thiết Kế: Họa tiết tinh xảo, đa dạng phong cách từ cổ điển đến hiện đại.\r\n- Ứng Dụng: Dễ dàng thay thế cho các loại dây thắt lưng phổ biến.\r\n\r\n **Gợi ý dịp tặng:** Lý tưởng làm quà tặng cho nam giới vào các dịp sinh nhật, Ngày Quốc tế Lao động.",
                        //    MinWeight = 250,
                        //    MaxWeight = 1200,
                        //    CategoryId = Guid.Parse("6551bec5-12eb-49e1-ba72-134545db85dc"), // Belt Buckles
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("belt_buckle.jpg"),
                        //            AttachmentAlt = "Mặt khóa thắt lưng sang trọng"
                        //        }
                        //    }
                        //},

                        //// Dịch vụ 14: Costume Gloves (Danh mục: Costume Gloves)
                        //new ServiceAddModel
                        //{

                        //    Name = "Găng Tay Biểu Diễn",
                        //    Description = "Tạo dấu ấn riêng với găng tay biểu diễn mang phong cách hoàng gia hoặc hiện đại. Phù hợp cho các dịp quan trọng như sự kiện, lễ hội.\r\n\r\nĐặc Điểm:\r\n- Chất Liệu: Satin, ren hoặc da cao cấp.\r\n- Thiết Kế: Dài hoặc ngắn tùy theo phong cách.\r\n- Ứng Dụng: Phù hợp với cosplay, biểu diễn nghệ thuật.\r\n\r\n **Gợi ý dịp tặng:** Món quà ý nghĩa cho những người yêu nghệ thuật, đặc biệt vào dịp Halloween hoặc các lễ hội thời trang.",
                        //    MinWeight = 100,
                        //    MaxWeight = 700,
                        //    CategoryId = Guid.Parse("947d3c1d-2e29-45ea-a965-d6dbc9d5a503"), // Costume Gloves
                        //    ServiceAttachments = new List<ServiceAttachmentAddModel>
                        //    {
                        //        new ServiceAttachmentAddModel
                        //        {
                        //            AttachmentUrl = CreateFormFileFromPath("costume_gloves.jpg"),
                        //            AttachmentAlt = "Găng tay biểu diễn thời trang"
                        //        }
                        //    }
                        //},
                        new ServiceAddModel
{
    Name = "Vòng Tay Tình Bạn",
    Description = "Vòng tay tình bạn là món quà tuyệt vời cho những người bạn thân thiết. Chất liệu từ dây thừng và kim loại, thiết kế đơn giản nhưng đầy ý nghĩa. Đây sẽ là món quà lý tưởng cho Ngày Quốc tế Hạnh phúc (20-03), mang đến niềm vui và sự kết nối giữa bạn bè.",
    MinWeight = 50,
    MaxWeight = 200,
    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
    ServiceAttachments = new List<ServiceAttachmentAddModel>
    {
        new ServiceAttachmentAddModel
        {
            AttachmentUrl = CreateFormFileFromPath("friendship_band.jpg"),
            AttachmentAlt = "Vòng tay tình bạn đơn giản"
        }
    }
},

new ServiceAddModel
{
    Name = "Chân Vòng Hoa Tết Nguyên Đán",
    Description = "Chân vòng hoa Tết Nguyên Đán là món quà trang trí tuyệt vời cho ngày Tết cổ truyền. Sản phẩm được làm từ hoa tươi và các vật liệu tự nhiên, thích hợp để trang trí trong gia đình vào dịp Tết Nguyên Đán (01-01).",
    MinWeight = 100,
    MaxWeight = 500,
    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
    ServiceAttachments = new List<ServiceAttachmentAddModel>
    {
        new ServiceAttachmentAddModel
        {
            AttachmentUrl = CreateFormFileFromPath("tet_flower_band.jpg"),
            AttachmentAlt = "Chân vòng hoa trang trí Tết"
        }
    }
},

new ServiceAddModel
{
    Name = "Vòng Tay Boho Summer",
    Description = "Vòng tay phong cách Boho đơn giản nhưng cực kỳ cá tính, mang đến vẻ đẹp tự nhiên và năng động. Phù hợp cho các buổi dã ngoại hoặc đi biển trong mùa hè, đặc biệt là cho Ngày Quốc tế Thiếu nhi (01-06).",
    MinWeight = 80,
    MaxWeight = 300,
    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
    ServiceAttachments = new List<ServiceAttachmentAddModel>
    {
        new ServiceAttachmentAddModel
        {
            AttachmentUrl = CreateFormFileFromPath("boho_summer_band.jpg"),
            AttachmentAlt = "Vòng tay Boho mùa hè"
        }
    }
},

new ServiceAddModel
{
    Name = "Vòng Tay Năng Lượng",
    Description = "Vòng tay năng lượng được chế tác từ đá tự nhiên, mang đến sự may mắn và năng lượng tích cực. Một món quà tuyệt vời cho Ngày Quốc tế Phụ nữ (08-03), giúp chị em cảm nhận được sự tự tin và mạnh mẽ.",
    MinWeight = 120,
    MaxWeight = 400,
    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
    ServiceAttachments = new List<ServiceAttachmentAddModel>
    {
        new ServiceAttachmentAddModel
        {
            AttachmentUrl = CreateFormFileFromPath("energy_band.jpg"),
            AttachmentAlt = "Vòng tay năng lượng tích cực"
        }
    }
},

new ServiceAddModel
{
    Name = "Vòng Tay Tình Yêu",
    Description = "Vòng tay tình yêu mang lại sự gắn kết cho các cặp đôi, được thiết kế tinh xảo và sang trọng. Đây sẽ là món quà hoàn hảo cho Ngày Giáng Sinh (24-12), thể hiện tình cảm yêu thương và sự chăm sóc.",
    MinWeight = 150,
    MaxWeight = 600,
    CategoryId = Guid.Parse("4a9d1e3b-5c67-4f4f-ad9c-9e6f7a8b0123"), // Arm Bands
    ServiceAttachments = new List<ServiceAttachmentAddModel>
    {
        new ServiceAttachmentAddModel
        {
            AttachmentUrl = CreateFormFileFromPath("love_band.jpg"),
            AttachmentAlt = "Vòng tay tình yêu"
        }
    }
},
//new ServiceAddModel
//{
//    Name = "Hoa Tai Giáng Sinh",
//    Description = "Hoa tai Giáng Sinh (24-12) với thiết kế ngôi sao và bông tuyết, mang đậm không khí lễ hội và sắc màu của mùa Giáng Sinh. Đây là món quà lý tưởng cho những ai yêu thích không khí ấm áp và vui tươi của lễ hội cuối năm.",
//    MinWeight = 50,
//    MaxWeight = 200,
//    CategoryId = Guid.Parse("b71db97b-7586-4262-b03c-8c8b93ec1539"), // Earrings
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("christmas_earrings.jpg"),
//            AttachmentAlt = "Hoa tai Giáng Sinh"
//        }
//    }
//},
//new ServiceAddModel
//{
//    Name = "Khăn Quàng Tết Đoan Ngọ",
//    Description = "Khăn quàng Tết Đoan Ngọ (05-05) với họa tiết thuyền rồng và hoa sen, tượng trưng cho sức mạnh và sự đoàn kết. Đây là món quà đặc biệt, phù hợp cho những dịp lễ hội truyền thống của Việt Nam.",
//    MinWeight = 150,
//    MaxWeight = 500,
//    CategoryId = Guid.Parse("b8dfb7fe-3eb8-44d5-b8f9-9b58e46c7d26"), // Scarves
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("dragon_boat_festival_scarf.jpg"),
//            AttachmentAlt = "Khăn quàng Tết Đoan Ngọ"
//        }
//    }
//},
//new ServiceAddModel
//{
//    Name = "Móc Khóa Ngày Quốc Tế Phụ Nữ",
//    Description = "Móc khóa Ngày Quốc Tế Phụ Nữ (08-03) với hình ảnh hoa hồng và biểu tượng nữ tính. Món quà này tượng trưng cho sự tôn vinh và yêu thương dành cho phụ nữ trong dịp lễ đặc biệt này.",
//    MinWeight = 30,
//    MaxWeight = 100,
//    CategoryId = Guid.Parse("f98e0a1a-049d-40e4-9f8b-d3c9842f4326"), // Keychains
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("womens_day_keychain.jpg"),
//            AttachmentAlt = "Móc khóa Ngày Quốc Tế Phụ Nữ"
//        }
//    }
//},
//new ServiceAddModel
//{
//    Name = "Trâm Cài Tóc Phong Cách Dân Gian",
//    Description = "Trâm cài tóc tinh xảo, mang đậm nét văn hóa dân tộc, phù hợp để sử dụng trong dịp **Giỗ Tổ Hùng Vương (10-03)**. Một món quà đặc biệt thể hiện sự kính trọng và lòng thành kính đối với các vua Hùng.",
//    MinWeight = 50,
//    MaxWeight = 250,
//    CategoryId = Guid.Parse("d2a2197d-6c93-489f-963b-45fb1b9a53b2"), // Costume Accessories
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("hairpin_traditional.jpg"),
//            AttachmentAlt = "Trâm cài tóc phong cách dân gian"
//        }
//    }
//},
//new ServiceAddModel
//{
//    Name = "Dây Chuyền Mặt Dây Chuyền Hình Tổ Quốc",
//    Description = "Dây chuyền với mặt dây hình bản đồ Việt Nam, thể hiện lòng yêu nước và sự tự hào dân tộc, rất phù hợp cho **Giỗ Tổ Hùng Vương (10-03)**. Một món quà đầy ý nghĩa để tưởng nhớ các vua Hùng, với thiết kế giản dị nhưng đầy sâu sắc.",
//    MinWeight = 100,
//    MaxWeight = 400,
//    CategoryId = Guid.Parse("be9a6ad4-e0a7-490b-93c9-b55975d87b8e"), // Necklaces
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("national_map_pendant.jpg"),
//            AttachmentAlt = "Mặt dây chuyền hình bản đồ Việt Nam"
//        }
//    }
//},
//new ServiceAddModel
//{
//    Name = "Mắt Kính Hình Mặt Trống Đồng",
//    Description = "Mắt kính thiết kế độc đáo với mặt trống đồng, một biểu tượng văn hóa dân tộc, rất phù hợp cho **Giỗ Tổ Hùng Vương (10-03)**. Đây là món quà đặc biệt cho những người yêu thích văn hóa lịch sử Việt Nam.",
//    MinWeight = 150,
//    MaxWeight = 600,
//    CategoryId = Guid.Parse("db76efbf-d61d-470d-8c7f-3c5b10c4c7f8"), // Costume Accessories
//    ServiceAttachments = new List<ServiceAttachmentAddModel>
//    {
//        new ServiceAttachmentAddModel
//        {
//            AttachmentUrl = CreateFormFileFromPath("trongdong_glasses.jpg"),
//            AttachmentAlt = "Mắt kính hình mặt trống đồng"
//        }
//    }
//}

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

                return Ok(new { Message = "Seeding successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = ex.Message
                });
            }
        }


        //    [Authorize]
        //    [HttpPost("seed-services")]
        //    public async Task<IActionResult> SeedPackages()
        //    {
        //        try
        //        {
        //            var serviceList = await _unitOfWork.ServiceRepository.GetTop14NewestServicesAsync();
        //            var packages = new List<PackageAddModel>
        //{
        //    new PackageAddModel
        //    {
        //        Name = PackageName.Basic, // Enum PackageName
        //        Description = "Gói dịch vụ tiêu chuẩn với thời gian xử lý 3-5 ngày làm việc.",
        //        Price = 500000,
        //        DeliveryTime = 5,
        //        SketchRevision = 1,
        //        ResponseTime = 24,
        //        MaxQuantity = 10,
        //    },
        //    new PackageAddModel
        //    {
        //        Name = PackageName.Standard,
        //        Description = "Gói dịch vụ cao cấp với thời gian xử lý nhanh trong 2-3 ngày.",
        //        Price = 1000000,
        //        DeliveryTime = 3,
        //        SketchRevision = 2,
        //        ResponseTime = 12,
        //        MaxQuantity = 5,
        //    },
        //    new PackageAddModel
        //    {
        //        Name = PackageName.Premium,
        //        Description = "Gói Dịch vụ VIP với ưu tiên xử lý ngay lập tức trong 24 giờ.",
        //        Price = 2000000,
        //        DeliveryTime = 1,
        //        SketchRevision = 3,
        //        ResponseTime = 6,
        //        MaxQuantity = 3,
        //    }
        //};
        //            string sourceLanguageCode = "vi";
        //            string targetLanguageCode = "en";
        //            foreach (var serviceId in serviceList)
        //            {
        //                var servicePackages = packages.Select(package => new PackageAddModel
        //                {
        //                    Name = package.Name,
        //                    Description =package.Description,
        //                    Price = package.Price + (new Random().Next(0, 500000)),
        //                    DeliveryTime = package.DeliveryTime + (new Random().Next(0, 2)),
        //                    SketchRevision = package.SketchRevision,
        //                    ResponseTime = package.ResponseTime,
        //                    MaxQuantity = package.MaxQuantity + (new Random().Next(1, 5)),
        //                }).ToList();

        //                foreach (var package in servicePackages)
        //                {
        //                    var result = await _serviceService.AddPackageAsync(package, serviceId, sourceLanguageCode, targetLanguageCode);
        //                    if (result.Code != StatusCodes.Status201Created)
        //                    {
        //                        return StatusCode(result.Code, result);
        //                    }
        //                }
        //            }
        //            return Ok(new { Message = "Seeding successfully" });
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, new
        //            {
        //                Error = ex.Message
        //            });
        //        }

        //    }

        [Authorize]
        [HttpPost("seed-package")]
        public async Task<IActionResult> SeedPackages()
        {
            try
            {
                var serviceList = await _unitOfWork.ServiceRepository.GetTop14NewestServicesAsync();
                var packages = new List<PackageAddModel>
        {
            new PackageAddModel
            {
                Name = PackageName.Basic,
                Description = "Gói dịch vụ tiêu chuẩn với thời gian xử lý 3-5 ngày làm việc.",
                Price = 100000,
                DeliveryTime = 5,
                SketchRevision = 1,
                //ResponseTime = 24,
                MaxQuantity = 10,
            },
            new PackageAddModel
            {
                Name = PackageName.Standard,
                Description = "Gói dịch vụ cao cấp với thời gian xử lý nhanh trong 3-5 ngày.",
                Price = 500000,
                DeliveryTime = 3,
                SketchRevision = 2,
                //ResponseTime = 12,
                MaxQuantity = 5,
            },
            new PackageAddModel
            {
                Name = PackageName.Premium,
                Description = "Gói dịch vụ VIP với ưu tiên xử lý ngay lập tức trong 1-2 ngày.",
                Price = 1000000,
                DeliveryTime = 2,
                SketchRevision = 3,
                //ResponseTime = 6,
                MaxQuantity = 3,
            }
        };

                var serviceFeatures = new Dictionary<string, List<string>>
{
    { "Khuyên Tai Đính Đá", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Khắc tên", "Kiểu dáng", "Chất liệu đá", "Độ sáng", "Số viên đá", "Loại đá", "Đặc điểm nổi bật" } },
    { "Dây Chuyền Cá Nhân Hóa", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Khắc tên", "Kiểu dáng", "Số hạt", "Loại hạt", "Chất liệu dây", "Đặc điểm nổi bật", "Phong cách" } },
    { "Nhẫn Đính Hôn", new List<string> { "Kích thước", "Chất liệu", "Kiểu dáng", "Chất liệu đá", "Loại đá", "Khắc tên", "Số viên đá", "Độ sáng", "Màu sắc", "Đặc điểm nổi bật" } },
    { "Đồng Hồ Cổ Điển", new List<string> { "Màu sắc", "Kích thước", "Chất liệu vỏ", "Chất liệu dây", "Loại mặt đồng hồ", "Kiểu dáng", "Màu sắc mặt đồng hồ", "Chất liệu kính", "Khắc tên", "Đặc điểm nổi bật" } },
    { "Kẹp Tiền Kim Loại", new List<string> { "Chất liệu", "Kích thước", "Khắc tên", "Màu sắc", "Phong cách", "Độ dày", "Kiểu dáng", "Chất liệu bao bì", "Đặc điểm nổi bật", "Màu sắc kim loại" } },
    { "Vòng Tay Hạt Đá", new List<string> { "Màu sắc", "Kích thước", "Chất liệu hạt", "Chất liệu dây", "Loại hạt", "Phong cách", "Độ dài", "Màu sắc dây", "Khắc tên", "Đặc điểm nổi bật" } },
    { "Gài Áo Thời Trang", new List<string> { "Màu sắc", "Chất liệu", "Kiểu dáng", "Phong cách", "Chất liệu kim loại", "Chất liệu vải", "Kích thước", "Khắc tên", "Đặc điểm nổi bật", "Đặc điểm thiết kế" } },
    { "Hộp Đựng Trang Sức Gỗ", new List<string> { "Chất liệu", "Kích thước", "Khắc tên", "Màu sắc", "Kiểu dáng", "Chất liệu gỗ", "Màu sắc gỗ", "Phong cách", "Đặc điểm nổi bật", "Đặc điểm thiết kế" } },
    { "Ví Chuỗi Thời Trang", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Kiểu dáng", "Khắc tên", "Số ngăn", "Chất liệu dây", "Đặc điểm thiết kế", "Phong cách", "Đặc điểm nổi bật" } },
    { "Giá Đỡ Nhẫn Nghệ Thuật", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Phong cách", "Kiểu dáng", "Chất liệu kim loại", "Chất liệu đá", "Khắc tên", "Đặc điểm nổi bật", "Đặc điểm thiết kế" } },
    { "Vòng Tay Bắp Tay Cá Tính", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Khắc tên", "Phong cách", "Chất liệu đá", "Loại đá", "Số viên đá", "Kiểu dáng", "Đặc điểm nổi bật" } },
    { "Mặt Khóa Thắt Lưng Độc Đáo", new List<string> { "Màu sắc", "Kích thước", "Chất liệu", "Kiểu dáng", "Chất liệu kim loại", "Phong cách", "Màu sắc kim loại", "Khắc tên", "Đặc điểm nổi bật", "Đặc điểm thiết kế" } },
    { "Găng Tay Biểu Diễn Sang Trọng", new List<string> { "Chất liệu", "Kích thước", "Màu sắc", "Phong cách", "Khắc tên", "Đặc điểm thiết kế", "Màu sắc vải", "Độ dài", "Đặc điểm nổi bật", "Chất liệu kim loại" } }
};


                var servicePackageList = new List<(PackageAddModel package, Service service)>();
                foreach (var service in serviceList)
                {
                    var tempPackageList = packages.Select(package => new PackageAddModel
                    {
                        Name = package.Name,
                        Description = package.Description,
                        Price = package.Price + (new Random().Next(0, 500000)),
                        DeliveryTime = package.DeliveryTime + (new Random().Next(0, 2)),
                        SketchRevision = package.SketchRevision,
                        ResponseTime = package.ResponseTime,
                        MaxQuantity = package.MaxQuantity + (new Random().Next(1, 5)),
                    }).ToList();

                    foreach (var package in tempPackageList)
                    {
                        servicePackageList.Add((package, service));
                    }
                }
                string sourceLanguageCode = "vi";
                string targetLanguageCode = "en";
                var addPackageResults = new List<PackageModel>();
                foreach (var (package, service) in servicePackageList)
                {
                    // Truyền thêm service tương ứng vào AddPackageAsync
                    var result = await _serviceService.AddPackageAsync(package, service.Id, sourceLanguageCode, targetLanguageCode);

                    if (result.Code == StatusCodes.Status201Created)
                    {
                        addPackageResults.Add((PackageModel)result.Data);
                    }
                    else
                    {
                        return StatusCode(result.Code, result);
                    }
                }

                foreach (var service in serviceList)
                {
                    var servicePackages = addPackageResults.Where(p => p.Name.ToString() == service.Name).ToList();

                    foreach (var package in servicePackages)
                    {
                        var features = serviceFeatures[service.Name].Select(featureName => new FeatureAddModel
                        {
                            PackageId = package.Id,  // Gán Id của gói
                            Name = featureName,  // Tên của tính năng
                            Question = $"Bạn muốn chọn {featureName} cho dịch vụ {service.Name}?",  // Câu hỏi động dựa trên tên tính năng và dịch vụ
                            QuestionType = MediaType.Select,  // Loại câu hỏi, chọn kiểu Select cho các tính năng
                            IsInformationRequired = true,  // Đảm bảo tính năng là bắt buộc
                            IsQuantity = false,
                            PackageFeatureAddModels = GeneratePackageFeatures(featureName, mediaType: "Select")
                        }).ToList();

                        // Thêm các tính năng vào Package
                        foreach (var feature in features)
                        {
                            var featureResult = await _featureService.AddFeatureAsync(feature, sourceLanguageCode, targetLanguageCode);
                            if (featureResult.Code != StatusCodes.Status201Created)
                            {
                                return StatusCode(featureResult.Code, featureResult);
                            }
                        }
                    }
                }
                return Ok(new { Message = "Seeding successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = ex.Message
                });
            }
        }

        private List<PackageFeatureAddModelForFeature> GeneratePackageFeatures(string featureName, string mediaType)
        {
            var random = new Random();
            var packageFeatures = new List<PackageFeatureAddModelForFeature>();

            // Chọn mediaType dựa trên các loại input khác nhau
            switch (mediaType)
            {
                case "Select":
                    switch (featureName)
                    {
                        case "Màu sắc":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Đỏ", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Xanh", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Tím", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vàng", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Đen", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Gỗ tự nhiên", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Kích thước":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Nhỏ (5-7 cm)", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vừa (8-10 cm)", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Lớn (11-15 cm)", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Siêu lớn (16 cm trở lên)", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Chất liệu vải":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vải cotton", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vải len", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vải lụa", IsExtra = true, AdditionalCost = 200000, AdditionalDay = 2, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vải thô", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Chất liệu dây":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Dây thừng", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Dây nylon", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Dây da", IsExtra = true, AdditionalCost = 150000, AdditionalDay = 1, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Chất liệu":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Bạc", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Thép không gỉ", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Titanium", IsExtra = true, AdditionalCost = 200000, AdditionalDay = 2, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Nhựa cao cấp", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Loại đá":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Ruby", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Emerald", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Diamond", IsExtra = true, AdditionalCost = 500000, AdditionalDay = 5, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Kiểu dáng":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Tròn", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Vuông", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Phong cách":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Cổ điển", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Hiện đại", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Tinh tế", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Nổi bật", IsExtra = true, AdditionalCost = 50000, AdditionalDay = 1, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Độ sáng":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Bình thường", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Cao", IsExtra = true, AdditionalCost = 20000, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            break;
                        case "Đặc điểm nổi bật":
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Chống nước", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Chống xước", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 0 });
                            packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "Thiết kế độc đáo", IsExtra = true, AdditionalCost = 100000, AdditionalDay = 1, IsChecked = false, MaxQuantity = 0 });
                            break;
                    }
                    break;

                case "Text":
                    if (featureName == "Khắc tên")
                    {
                        packageFeatures.Add(new PackageFeatureAddModelForFeature { Name = "CVT", IsExtra = false, AdditionalCost = 0, AdditionalDay = 0, IsChecked = false, MaxQuantity = 1 });
                    }
                    break;


                default:
                    throw new ArgumentException("Unsupported media type.");
            }

            return packageFeatures;
        }



        private static IFormFile CreateFormFileFromPath(string fileName)
        {
            string rootPath = Directory.GetCurrentDirectory(); // Lấy thư mục gốc của API
            string filePath = Path.Combine(rootPath, "Images", fileName); // Đường dẫn đầy đủ

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File không tồn tại: {filePath}");
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, "AttachmentUrl", Path.GetFileName(filePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }


    }

}
