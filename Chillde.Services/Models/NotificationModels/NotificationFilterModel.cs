using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.NotificationModels
{
    public class NotificationFilterModel : FilterParameter
    {
        public Guid? AccountId { get; set; }
        public NotificationType? NotificationType { get; set; }
    }
}
