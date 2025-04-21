using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Interfaces
{
    public interface INotificationContentRepository : IGenericRepository<NotificationContent>
    {
        Task<NotificationContent?> GetByKeyAsync(NotificationCode key);
    }
}
