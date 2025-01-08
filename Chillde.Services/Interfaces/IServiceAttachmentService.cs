using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceAttachmentModels;

namespace Chillde.Services.Interfaces
{
    public interface IServiceAttachmentService
    {
        Task<ResponseModel> AddRangeAsync(Guid serviceId, ServiceAttachmentAddModel model);
    }
}
