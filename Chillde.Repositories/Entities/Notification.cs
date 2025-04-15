using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Notification : BaseEntity
    {
        //public string Key { get; set; } //Dùng để gom thông báo lại với nhau. Ví dụ: A và 3 người khác đã đánh giá dịch vụ của bạn.
        public required string Content { get; set; }
        public Guid AccountId { get; set; } //Id người nhận
        public Guid NotificationTypeId { get; set; }
        public Guid SourceId { get; set; } //Id của Entity cần thông báo. Ví dụ: OrderId, FeedbackId,...
        public string? ImageUrl { get; set; }

        public Account Account { get; set; }
        public NotificationContent NotificationType { get; set; }
    }
}
