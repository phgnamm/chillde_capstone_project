using Chillde.Repositories.Models.NotificationModels;
using Chillde.Services.Models.NotificationModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ResponseModel> PushNotification(NotificationAddModel notificationAddModel);
        Task<ResponseModel> GetAll(NotificationFilterModel notificationFilterModel);
        Task<ResponseModel> UpdateIsReadAsync(Guid notificationId);
    }
}
