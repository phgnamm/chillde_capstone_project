namespace Chillde.Repositories.Models.NotificationModels
{
    public class NotificationAddModel
    {
        public required string Content { get; set; }
        public Guid AccountId { get; set; } //Id người nhận
        public Guid NotificationContentId { get; set; }
        public Guid SourceId { get; set; } //Id của Entity cần thông báo. Ví dụ: OrderId, FeedbackId,...
    }
}
