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
using Chillde.Repositories.Models.ServiceModels;
using OpenAI.GPT3.Managers;

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
            int gracePeriodDays = 2;
            int maxDaysThreshold = 45;
            var eventList = new List<EventModel>
{
    new EventModel
    {
        NameVi = "Tết Dương lịch", NameEn = "New Year's Day", Date = "01-01", IsLunar = false,
        KeywordsVi = new List<string> { "Tết", "Năm mới", "Ngày lễ", "Chúc mừng", "Gia đình", "Du lịch", "Tiệc tùng", "Mừng năm mới", "Chúc phúc", "Lịch nghỉ" },
        KeywordsEn = new List<string> { "New Year", "Celebration", "Holiday", "Family", "Travel", "Party", "Greetings", "Festivity", "New Year's Eve", "Festive season" },
        Detail = "Tết Dương lịch là dịp lễ đầu năm diễn ra vào ngày 1/1. Đây là thời điểm người dân tìm kiếm các sản phẩm thủ công trang trí nhà cửa như đèn lồng handmade, thiệp chúc mừng năm mới tự làm, và các vật phẩm may mắn. Nhiều người cũng quan tâm đến các workshop làm đồ trang trí tết, túi may mắn thủ công, và các bộ quà tặng handmade ý nghĩa cho người thân."
    },
    new EventModel
    {
        NameVi = "Ngày Thầy thuốc Việt Nam", NameEn = "Vietnamese Doctors' Day", Date = "27-02", IsLunar = false,
        KeywordsVi = new List<string> { "Thầy thuốc", "Y tế", "Bác sĩ", "Chăm sóc sức khỏe", "Y học", "Tôn vinh", "Người thầy thuốc", "Ngày truyền thống", "Cảm ơn", "Chăm sóc" },
        KeywordsEn = new List<string> { "Doctors", "Healthcare", "Medical profession", "Doctors' Day", "Vietnamese doctors", "Medicine", "Appreciation", "Honoring", "Health", "Gratitude" },
        Detail = "Ngày Thầy thuốc Việt Nam (27/2) là dịp để bày tỏ lòng biết ơn đối với các bác sĩ và nhân viên y tế. Các sản phẩm thủ công như thiệp cảm ơn handmade, hoa giấy trang trí, hộp quà tự làm được nhiều người tìm kiếm. Các workshop làm quà tặng đặc biệt cho ngành y như túi dụng cụ y tế thủ công, sổ tay bọc vải, hoặc các móc khóa handmade cũng nhận được nhiều sự quan tâm"
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Phụ nữ", NameEn = "International Women's Day", Date = "08-03", IsLunar = false,
        KeywordsVi = new List<string> { "Phụ nữ", "Quyền phụ nữ", "Bình đẳng giới", "Tôn vinh", "Ngày lễ", "Chúc mừng", "Tự do", "Độc lập", "Người phụ nữ", "Vượt qua" },
        KeywordsEn = new List<string> { "Women", "Gender equality", "Women's rights", "Celebration", "Empowerment", "Equality", "Feminism", "Strength", "Respect", "Inspiration" },
        Detail = "Ngày Quốc tế Phụ nữ (8/3) là dịp để tôn vinh phụ nữ mọi lứa tuổi. Các sản phẩm thủ công như hoa vải, trang sức handmade, túi xách thêu tay, thiệp 3D và các bộ quà tặng DIY được ưa chuộng. Nhiều người tìm kiếm các workshop làm quà handmade, dịch vụ đóng gói quà tặng sáng tạo, hoặc các bộ kit tự làm quà tặng tại nhà để thể hiện tình cảm một cách độc đáo."
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Hạnh phúc", NameEn = "International Day of Happiness", Date = "20-03", IsLunar = false,
        KeywordsVi = new List<string> { "Hạnh phúc", "Niềm vui", "Cuộc sống", "Ngày lễ", "Chia sẻ", "Cộng đồng", "Tinh thần", "Phúc lợi", "Lạc quan", "Tươi cười" },
        KeywordsEn = new List<string> { "Happiness", "Joy", "Well-being", "Celebration", "Optimism", "Life", "Community", "Smile", "Mental health", "Positivity" },
        Detail = "Ngày Quốc tế Hạnh phúc (20/3) là dịp để mọi người chia sẻ niềm vui và hạnh phúc. Các sản phẩm thủ công mang thông điệp tích cực như sổ ghi chép biết ơn handmade, hộp kỷ niệm DIY, đồ trang trí nhà cửa mang chủ đề hạnh phúc rất được ưa chuộng. Các workshop làm đồ handmade theo nhóm để tăng kết nối cộng đồng cũng thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Giỗ Tổ Hùng Vương", NameEn = "Hung Kings' Commemoration Day", Date = "10-03", IsLunar = true,
        KeywordsVi = new List<string> { "Hùng Vương", "Giỗ tổ", "Lịch sử", "Dân tộc", "Truyền thống", "Tôn vinh", "Giới thiệu văn hóa", "Phong tục", "Lễ hội", "Tổ tiên" },
        KeywordsEn = new List<string> { "Hung Kings", "Commemoration", "Tradition", "History", "Culture", "National identity", "Ancestor worship", "Festival", "Heritage", "Ceremony" },
        Detail = "Giỗ Tổ Hùng Vương (10/3 âm lịch) là dịp để tưởng nhớ tổ tiên và giáo dục truyền thống. Các sản phẩm thủ công truyền thống như nón lá trang trí, đồ thủ công mỹ nghệ từ tre trúc, tranh dân gian được nhiều người tìm kiếm. Các workshop làm bánh truyền thống, đồ trang trí lễ hội thủ công, và các vật phẩm cúng giỗ handmade cũng nhận được nhiều sự quan tâm."
    },
    new EventModel
    {
        NameVi = "Ngày Giải phóng miền Nam", NameEn = "Reunification Day", Date = "30-04", IsLunar = false,
        KeywordsVi = new List<string> { "Giải phóng", "Miền Nam", "Ngày chiến thắng", "Tự do", "Hòa bình", "Lịch sử", "Dân tộc", "Chung tay", "Ngày lễ", "Tinh thần đoàn kết" },
        KeywordsEn = new List<string> { "Reunification", "Victory", "Freedom", "Independence", "History", "South Vietnam", "Peace", "Unity", "National Day", "Celebration" },
        Detail = "Ngày Giải phóng miền Nam (30/4) là dịp kỷ niệm quan trọng về lịch sử đất nước. Các sản phẩm thủ công như cờ Tổ quốc handmade, tranh vẽ chủ đề lịch sử, và các vật phẩm kỷ niệm được tìm kiếm nhiều. Các workshop làm đồ lưu niệm chủ đề đoàn kết dân tộc, album ảnh lịch sử handmade, và các vật phẩm trang trí ngày lễ thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Lao động", NameEn = "International Workers' Day", Date = "01-05", IsLunar = false,
        KeywordsVi = new List<string> { "Lao động", "Công nhân", "Ngày lễ", "Tôn vinh", "Công bằng", "Công lý", "Quyền lợi", "Chia sẻ", "Xã hội", "Nỗ lực" },
        KeywordsEn = new List<string> { "Labor", "Workers", "International Day", "Social justice", "Rights", "Fairness", "Dignity", "Celebration", "Workforce", "Unity" },
        Detail = "Ngày Quốc tế Lao động (1/5) là dịp để tôn vinh người lao động các ngành nghề. Các sản phẩm thủ công như vật dụng trang trí văn phòng handmade, quà tặng đồng nghiệp tự làm, và các vật phẩm trang trí cho không gian làm việc được tìm kiếm nhiều. Các workshop làm đồ thủ công giúp giảm stress công việc cũng nhận được sự quan tâm."
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Thiếu nhi", NameEn = "International Children's Day", Date = "01-06", IsLunar = false,
        KeywordsVi = new List<string> { "Trẻ em", "Ngày Quốc tế", "Tổ chức", "Phúc lợi trẻ em", "Bảo vệ trẻ em", "Niềm vui", "Học tập", "Chơi đùa", "Chăm sóc", "Tương lai" },
        KeywordsEn = new List<string> { "Children", "International Day", "Rights of children", "Protection", "Joy", "Education", "Future", "Care", "Play", "Family" },
        Detail = "Ngày Quốc tế Thiếu nhi (1/6) là dịp để tổ chức các hoạt động vui chơi cho trẻ em. Các sản phẩm thủ công như đồ chơi handmade, búp bê vải, thiệp pop-up, và bộ kit DIY cho trẻ em được tìm kiếm nhiều. Các workshop làm đồ chơi sáng tạo, workshop vẽ và tô màu, cũng như các hoạt động thủ công gia đình thu hút nhiều gia đình tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Người cao tuổi", NameEn = "International Day of Older Persons", Date = "01-10", IsLunar = false,
        KeywordsVi = new List<string> { "Người cao tuổi", "Tôn vinh", "Lão hóa", "Chăm sóc", "Phúc lợi", "Lão khoa", "Gia đình", "Tương lai", "Ngày lễ", "Sức khỏe" },
        KeywordsEn = new List<string> { "Older persons", "Elderly", "Celebration", "Aging", "Care", "Respect", "Family", "Social welfare", "Health", "Dignity" },
        Detail = "Ngày Quốc tế Người cao tuổi (1/10) là dịp để tôn vinh các bậc cao niên. Các sản phẩm thủ công như album ảnh handmade, thiệp chúc sức khỏe, quà tặng thân thiện với người cao tuổi như gối thêu, túi đựng thuốc thủ công được tìm kiếm nhiều. Các workshop làm đồ thủ công phù hợp với người cao tuổi như đan len, thêu thùa cũng nhận được sự quan tâm."
    },
    new EventModel
    {
        NameVi = "Ngày Giải phóng Thủ đô", NameEn = "Hanoi Liberation Day", Date = "10-10", IsLunar = false,
        KeywordsVi = new List<string> { "Giải phóng", "Thủ đô", "Hà Nội", "Lịch sử", "Ngày chiến thắng", "Tự do", "Chúng ta", "Đoàn kết", "Bảo vệ", "Lịch sử dân tộc" },
        KeywordsEn = new List<string> { "Liberation", "Hanoi", "Victory", "Freedom", "National history", "Reunification", "Victory day", "Patriotism", "Independence", "Unity" },
        Detail = "Ngày Giải phóng Thủ đô (10/10) là dịp kỷ niệm lịch sử quan trọng của Hà Nội. Các sản phẩm thủ công mang đặc trưng Hà Nội như tranh làng nghề, đồ lưu niệm handmade về 36 phố phường, và các vật phẩm kỷ niệm được tìm kiếm nhiều. Các workshop làm đồ thủ công truyền thống Hà Nội và tour trải nghiệm các làng nghề thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Halloween", NameEn = "Halloween", Date = "31-10", IsLunar = false,
        KeywordsVi = new List<string> { "Halloween", "Ma quái", "Trang trí", "Lễ hội", "Hóa trang", "Sợ hãi", "Trick or treat", "Lễ hội Mỹ", "Chơi đùa", "Đêm hội" },
        KeywordsEn = new List<string> { "Halloween", "Spooky", "Costumes", "Trick or treat", "Decoration", "Festival", "Frightening", "Scary", "Night", "Celebration" },
        Detail = "Halloween (31/10) là dịp lễ hội mang phong cách phương Tây. Các sản phẩm thủ công như mặt nạ handmade, đồ trang trí bí ngô, trang phục hóa trang DIY, và các vật phẩm trang trí nhà cửa theo chủ đề ma quái được tìm kiếm nhiều. Các workshop làm đồ handmade cho Halloween, trang trí bí ngô, và làm kẹo thủ công thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Nhà giáo Việt Nam", NameEn = "Vietnamese Teachers' Day", Date = "20-11", IsLunar = false,
        KeywordsVi = new List<string> { "Nhà giáo", "Giáo viên", "Tôn vinh", "Ngày lễ", "Cảm ơn", "Truyền cảm hứng", "Học sinh", "Phát triển", "Giáo dục", "Kỷ niệm" },
        KeywordsEn = new List<string> { "Teachers", "Education", "Teaching", "Appreciation", "Honor", "Inspiration", "Gratitude", "Students", "Learning", "Celebration" },
         Detail = "Ngày Nhà giáo Việt Nam (20/11) là dịp để tôn vinh các thầy cô giáo. Các sản phẩm thủ công như thiệp cảm ơn handmade, hoa giấy, quà tặng DIY dành cho giáo viên như bút được trang trí thủ công, sổ tay bọc vải, và các vật phẩm trang trí bàn học được tìm kiếm nhiều. Các workshop làm quà tặng thầy cô cũng thu hút nhiều học sinh tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Quốc tế Nam giới", NameEn = "International Men's Day", Date = "19-11", IsLunar = false,
        KeywordsVi = new List<string> { "Nam giới", "Tôn vinh", "Ngày lễ", "Công bằng", "Giới tính", "Bình đẳng", "Sức khỏe", "Gia đình", "Hạnh phúc", "Phát triển" },
        KeywordsEn = new List<string> { "Men", "Gender equality", "Men's Day", "Celebration", "Health", "Family", "Well-being", "Social justice", "Community", "Respect" },
        Detail = "Ngày Quốc tế Nam giới (19/11) là dịp để tôn vinh phái mạnh. Các sản phẩm thủ công như ví da handmade, móc khóa khắc tên, hộp đựng đồng hồ tự làm, và các vật dụng cá nhân được tìm kiếm nhiều. Các workshop làm đồ da, làm đồ gỗ DIY, và các hoạt động thủ công theo sở thích nam giới cũng nhận được sự quan tâm."
    },
    new EventModel
    {
        NameVi = "Ngày Quân đội Nhân dân Việt Nam", NameEn = "Vietnam People's Army Day", Date = "22-12", IsLunar = false,
        KeywordsVi = new List<string> { "Quân đội", "Nhân dân", "Ngày lễ", "Tôn vinh", "Anh hùng", "Bảo vệ tổ quốc", "Tinh thần", "Dân tộc", "Quân nhân", "Chiến tranh" },
        KeywordsEn = new List<string> { "Army", "People's Army", "Vietnam", "Soldiers", "Commemoration", "Victory", "Nation", "Defense", "Heroes", "Military" },
        Detail = "Ngày Quân đội Nhân dân Việt Nam (22/12) là dịp để tôn vinh các chiến sĩ. Các sản phẩm thủ công như thiệp cảm ơn handmade, album ảnh DIY, vật phẩm kỷ niệm mang chủ đề quân đội, và các món quà tặng thủ công được tìm kiếm nhiều. Các workshop làm đồ handmade mang tính kỷ niệm và các hoạt động trang trí đặc biệt thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Giáng sinh (Noel)", NameEn = "Christmas Eve", Date = "24-12", IsLunar = false,
        KeywordsVi = new List<string> { "Giáng sinh", "Noel", "Lễ hội", "Tôn vinh", "Tình yêu", "Gia đình", "Mừng Chúa Giáng sinh", "Món quà", "Lễ hội mùa đông", "Tổ chức" },
        KeywordsEn = new List<string> { "Christmas", "Eve", "Holiday", "Celebration", "Family", "Love", "Gifts", "Religion", "Winter festival", "Togetherness" },
         Detail = "Giáng sinh (24/12) là dịp lễ hội lớn với nhiều hoạt động trang trí và tặng quà. Các sản phẩm thủ công như cây thông Noel mini handmade, vòng nguyệt quế, thiệp Giáng sinh 3D, đồ trang trí nhà cửa, tất Noel, và các bộ quà tặng DIY được tìm kiếm nhiều. Các workshop làm đồ trang trí Giáng sinh, làm bánh cookies, và đóng gói quà tặng sáng tạo thu hút nhiều gia đình tham gia."
    },
    new EventModel
    {
        NameVi = "Tết Nguyên Đán", NameEn = "Lunar New Year", Date = "01-01", IsLunar = true,
        KeywordsVi = new List<string> { "Tết", "Lịch Nguyên Đán", "Tết cổ truyền", "Mừng năm mới", "Lễ hội", "Gia đình", "Tặng quà", "Chúc mừng", "Tượng trưng", "Phong tục" },
        KeywordsEn = new List<string> { "Lunar New Year", "Tet", "Traditional", "Festivity", "New Year", "Celebration", "Family", "Culture", "Tradition", "Gifts" },
        Detail = "Tết Nguyên Đán (mùng 1 tháng 1 âm lịch) là lễ hội truyền thống lớn nhất trong năm. Các sản phẩm thủ công như tranh Tết, đèn lồng handmade, thiệp chúc Tết, đồ trang trí nhà cửa thủ công, bao lì xì handmade, và các vật phẩm may mắn được tìm kiếm nhiều. Các workshop làm bánh chưng, trang trí nhà cửa đón Tết, và làm đồ handmade mang ý nghĩa năm mới thu hút nhiều gia đình tham gia."
    },
    new EventModel
    {
        NameVi = "Lễ Vu Lan", NameEn = "Vu Lan Festival", Date = "15-07", IsLunar = true,
        KeywordsVi = new List<string> { "Vu Lan", "Báo hiếu", "Tôn vinh", "Lễ hội", "Cúng tổ tiên", "Phúc đức", "Tình mẫu tử", "Hiếu hạnh", "Cộng đồng", "Tâm linh" },
        KeywordsEn = new List<string> { "Vu Lan", "Ancestral worship", "Filial piety", "Mother's love", "Festival", "Respect", "Tradition", "Honor", "Spiritual", "Cultural celebration" },
        Detail = "Lễ Vu Lan (15/7 âm lịch) là dịp để tôn vinh công ơn cha mẹ. Các sản phẩm thủ công như hoa hồng vải, thiệp tri ân handmade, quà tặng DIY dành cho cha mẹ, và các vật phẩm cúng lễ thủ công được tìm kiếm nhiều. Các workshop làm đồ handmade mang ý nghĩa báo hiếu và các hoạt động thủ công gia đình thu hút nhiều người tham gia."
    },
    new EventModel
    {
        NameVi = "Ngày Lễ Tình nhân",
        NameEn = "Valentine's Day",
        Date = "14-02",
        IsLunar = false,
        KeywordsVi = new List<string> { "Tình yêu", "Lễ tình nhân", "Quà tặng", "Hẹn hò", "Chúc mừng", "Đôi lứa", "Lãng mạn", "Cảm ơn", "Chia sẻ", "Tình cảm" },
        KeywordsEn = new List<string> { "Love", "Valentine", "Gifts", "Couple", "Romance", "Affection", "Celebration", "Sharing", "Sweetheart" },
        Detail = "Ngày Valentine (14/2) là dịp để thể hiện tình yêu. Các sản phẩm thủ công như hoa giấy handmade, thiệp tình yêu pop-up, album ảnh DIY, hộp quà trái tim, và các món quà thủ công cá nhân hóa được tìm kiếm nhiều. Các workshop làm quà tặng đôi, làm socola handmade, và trang trí quà tặng sáng tạo thu hút nhiều cặp đôi tham gia."
    },
    new EventModel
{
    NameVi = "Ngày Quốc Khánh",
    NameEn = "Independence Day",
    Date = "02-09",
    IsLunar = false,
    KeywordsVi = new List<string> { "Độc lập", "Lịch sử", "Tự do", "Chủ quyền", "Tổ quốc", "Hòa bình", "Tự hào", "Cảm ơn", "Tưởng niệm", "Lễ hội","Quôc Khánh" },
    KeywordsEn = new List<string> { "Independence", "Freedom", "National Day", "Sovereignty", "Patriotism", "Country", "Pride", "Celebration" },
    Detail = "Ngày Quốc Khánh (2/9) là dịp để kỷ niệm ngày độc lập của đất nước. Các sản phẩm thủ công như cờ Tổ quốc handmade, tranh dân gian, vật phẩm kỷ niệm mang chủ đề dân tộc, và các món đồ trang trí được tìm kiếm nhiều. Các workshop làm đồ handmade mang tính truyền thống, các hoạt động thủ công gia đình, và các buổi trưng bày sản phẩm làng nghề thu hút nhiều người tham gia."
},
    new EventModel
{
    NameVi = "Ngày Thế giới Môi trường",
    NameEn = "World Environment Day",
    Date = "05-06",
    IsLunar = false,
    KeywordsVi = new List<string> { "Môi trường", "Bảo vệ thiên nhiên", "Bảo tồn", "Năng lượng xanh", "Chống ô nhiễm", "Khí hậu", "Sự sống", "Đa dạng sinh học", "Hành động" },
    KeywordsEn = new List<string> { "Environment", "Nature", "Conservation", "Green energy", "Pollution", "Climate", "Biodiversity", "Action", "Protection" },
    Detail = "Ngày Thế giới Môi trường (5/6) là dịp để nâng cao nhận thức về bảo vệ môi trường. Các sản phẩm thủ công từ vật liệu tái chế như túi vải, đồ trang trí từ chai nhựa, sản phẩm zero-waste handmade được tìm kiếm nhiều. Các workshop làm đồ handmade thân thiện với môi trường, tận dụng vật liệu tái chế, và các hoạt động trồng cây thu hút nhiều người tham gia."
},
    new EventModel
{
    NameVi = "Ngày Thương binh Liệt sĩ",
    NameEn = "War Invalids and Martyrs Day",
    Date = "27-07",
    IsLunar = false,
    KeywordsVi = new List<string> { "Thương binh", "Liệt sĩ", "Cảm ơn", "Kỷ niệm", "Hy sinh", "Hòa bình", "Tưởng niệm" },
    KeywordsEn = new List<string> { "Invalids", "Martyrs", "Gratitude", "Remembrance", "Sacrifice", "Peace", "Commemoration" },
    Detail = "Ngày Thương binh Liệt sĩ (27/7) là dịp để tôn vinh sự hy sinh của các anh hùng. Các sản phẩm thủ công như thiệp tri ân handmade, vòng hoa giấy, album ảnh kỷ niệm DIY, và các vật phẩm trang trí được tìm kiếm nhiều. Các workshop làm đồ handmade mang tính kỷ niệm và các hoạt động sáng tạo tập thể mang ý nghĩa tri ân thu hút nhiều người tham gia."
},
    new EventModel
{
    NameVi = "Ngày Lễ hội Trung thu",
    NameEn = "Mid-Autumn Festival",
    Date = "15-08",
    IsLunar = true,
    KeywordsVi = new List<string> { "Trung thu", "Lễ hội", "Tết Trung thu", "Đêm trăng", "Mâm cỗ", "Lồng đèn", "Tết thiếu nhi", "Gia đình" },
    KeywordsEn = new List<string> { "Mid-Autumn", "Festival", "Moon cake", "Lanterns", "Children", "Family", "Harvest" },
    Detail = "Lễ hội Trung thu (15/8 âm lịch) là dịp vui chơi đặc biệt cho trẻ em. Các sản phẩm thủ công như lồng đèn handmade, đèn ông sao DIY, mặt nạ giấy, đồ chơi truyền thống, và các vật phẩm trang trí được tìm kiếm nhiều. Các workshop làm bánh trung thu, làm lồng đèn, và các hoạt động thủ công gia đình thu hút nhiều gia đình tham gia."
},
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
            var keywordsVi = e.KeywordsVi != null ? string.Join(", ", e.KeywordsVi) : string.Empty;
            var keywordsEn = e.KeywordsEn != null ? string.Join(", ", e.KeywordsEn) : string.Empty;
            return new { EventVi = e.NameVi, EventEn = e.NameEn, Date = eventDate, KeywordsVi = keywordsVi, KeywordsEn = keywordsEn, Detail = e.Detail };
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
                    Data = new
                    {
                        pastRecentEvent.EventVi,
                        pastRecentEvent.EventEn,
                        Date = pastRecentEvent.Date.ToString("yyyy-MM-dd"),
                        KeywordsVi = pastRecentEvent.KeywordsVi,
                        KeywordsEn = pastRecentEvent.KeywordsEn,
                        Detail = pastRecentEvent.Detail,
                    }
                };
            }
            if (nearestEvent != null && (nearestEvent.Date - currentDate).TotalDays <= maxDaysThreshold)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = sourLanguageCode == "vi" ? "Lấy sự kiện thành công" : "Get Event Successfully",
                    Data = new
                    {
                        nearestEvent.EventVi,
                        nearestEvent.EventEn,
                        Date = nearestEvent.Date.ToString("yyyy-MM-dd"),
                        KeywordsVi = nearestEvent.KeywordsVi,
                        KeywordsEn = nearestEvent.KeywordsEn,
                        Detail = nearestEvent.Detail,
                    }
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status404NotFound,
                Message = sourLanguageCode == "vi" ? "Không có sự kiện nào sắp diễn ra." : "No upcoming events.",
                Data = null
            };
        }
        public async Task<List<ServiceModel>> RerankTopServicesWithGPTAsync(
            string eventDescription,
            List<ServiceModel> candidates,
            int topN = 10)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Sự kiện: {eventDescription}\n");
            promptBuilder.AppendLine("Dưới đây là danh sách dịch vụ:");

            for (int i = 0; i < candidates.Count; i++)
            {
                var service = candidates[i];
                promptBuilder.AppendLine($"{i + 1}. Tên: {service.Name}");
                promptBuilder.AppendLine($"   Mô tả: {service.Description}");
            }

            promptBuilder.AppendLine($"\nHãy chọn ra {topN} dịch vụ mà bạn cho rằng nó phù hợp và có thể dùng cho sự kiện đó, có thể không hoàn toàn phù hợp nhưng về ngữ cảnh và tác dụng vẫn có thể liên quan.");
            promptBuilder.AppendLine("Chỉ trả về danh sách số thứ tự (ví dụ: 3, 5, 1, ...).");

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
            new { role = "system", content = "Bạn là một chuyên gia tư vấn dịch vụ cho sự kiện." },
            new { role = "user", content = promptBuilder.ToString() }
        },
                temperature = 0.2
            };

            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API key is missing.");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var completionResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var content = completionResult?.choices[0]?.message?.content?.ToString();

            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("No valid response from OpenAI API.");
            }

            var indexes = new List<int>();

            foreach (var s in content.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(s.Trim(), out int index) && index > 0)
                {
                    index = index - 1;
                    if (index >= 0 && index < candidates.Count)
                    {
                        indexes.Add(index);
                    }
                }
            }

            var selectedServices = new List<ServiceModel>();
            foreach (var index in indexes.Take(topN))
            {
                selectedServices.Add(candidates[index]);
            }

            return selectedServices;
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
                model = "text-embedding-3-small",
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
            var sb = new StringBuilder();
            sb.AppendLine("Bạn sẽ đóng vai một chuyên gia về sản phẩm thủ công.");
            sb.AppendLine("Dựa trên danh mục và mô tả sản phẩm được cung cấp, hãy phân tích và trích xuất đầy đủ các thuộc tính hợp lý nhất dưới dạng một mảng JSON có cấu trúc.");
            sb.AppendLine();
            sb.AppendLine($"Danh mục sản phẩm: {prompt[0]}");
            sb.AppendLine($"Mô tả chi tiết: {prompt[1]}");
            sb.AppendLine();
            sb.AppendLine("Yêu cầu định dạng đầu ra:");
            sb.AppendLine("- Mỗi phần tử trong mảng là một thuộc tính sản phẩm, bao gồm các trường:");
            sb.AppendLine("  - `tên` (string): Tên thuộc tính, được viết bằng ngôn ngữ đơn giản, dễ hiểu cho khách hàng.");
            sb.AppendLine("  - `loại` (integer): Kiểu dữ liệu của thuộc tính, với các giá trị:");
            sb.AppendLine("      - 0: Văn bản tự do (ví dụ: mô tả chi tiết).");
            sb.AppendLine("      - 1: Số (ví dụ: trọng lượng, kích thước, giá trị cụ thể).");
            sb.AppendLine("      - 2: Lựa chọn (một lựa chọn duy nhất, ví dụ: chất liệu, màu sắc, kiểu dáng).");
            sb.AppendLine("      - 6: Hộp kiểm (nhiều lựa chọn, ví dụ: tính năng, dịp sử dụng).");
            sb.AppendLine("  - `tùy chọn` (array of strings): Danh sách các tùy chọn, bắt buộc nếu `loại` là 2 hoặc 6.");
            sb.AppendLine();
            sb.AppendLine("Nguyên tắc tạo thuộc tính:");
            sb.AppendLine("- Viết rõ ràng, gần gũi, phù hợp với người dùng phổ thông.");
            sb.AppendLine("- Ưu tiên thông tin từ mô tả, đồng thời suy luận thêm nếu cần thiết để hoàn chỉnh bộ thuộc tính.");
            sb.AppendLine("- Các câu hỏi có/không nên được biểu diễn bằng `loại: 2` với `tùy chọn: ['Có', 'Không']`.");
            sb.AppendLine();
            sb.AppendLine("Phạm vi thuộc tính cần xem xét bao gồm (nhưng không giới hạn):");
            sb.AppendLine("- Đặc điểm vật lý (kích thước, trọng lượng, màu sắc, chất liệu, hình dạng)");
            sb.AppendLine("- Tính năng và công dụng (chức năng sử dụng, đối tượng sử dụng, dịp sử dụng)");
            sb.AppendLine("- Khả năng cá nhân hóa (in tên, chọn màu, tùy chỉnh kích thước)");
            sb.AppendLine("- Tính bền vững và an toàn (nguồn gốc nguyên liệu, thân thiện môi trường, an toàn khi sử dụng)");
            sb.AppendLine("- Thông tin quy trình sản xuất (thủ công, tái chế, chứng nhận nếu có)");
            sb.AppendLine("- Bao bì và đóng gói (có hộp quà, vật liệu tái chế, v.v.)");
            sb.AppendLine();
            sb.AppendLine("⚠️ Không bao gồm các thuộc tính sau:");
            sb.AppendLine("- Tên sản phẩm");
            sb.AppendLine("- Mô tả sản phẩm");
            sb.AppendLine("- Ngân sách tối thiểu / tối đa");
            sb.AppendLine("- Thời gian / deadline");
            sb.AppendLine("- Số lượng");
            sb.AppendLine();
            sb.AppendLine("📤 Đầu ra yêu cầu là một mảng JSON có cấu trúc đúng, không kèm theo giải thích hoặc mô tả bên ngoài.");

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
