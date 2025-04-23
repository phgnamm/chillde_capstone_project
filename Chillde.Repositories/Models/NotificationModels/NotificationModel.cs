using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.NotificationModels
{
    public class NotificationModel : BaseEntity
    {
        public required string Content { get; set; }
        public Guid AccountId { get; set; } //Id người nhận
        public Guid NotificationContentId { get; set; }
        public NotificationType Type { get; set; }
        public Guid SourceId { get; set; } //Id của Entity cần thông báo. Ví dụ: OrderId, FeedbackId,...
        public string? ImageUrl { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
