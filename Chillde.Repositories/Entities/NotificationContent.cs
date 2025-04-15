using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class NotificationContent :BaseEntity
    {
        public NotificationType
            Type { get; set; }
        public required string Content { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
