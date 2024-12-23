using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IServiceService
    {
        Task<ResponseModel> Add(PackageAddModel packageAddModel, Guid serviceId);        
    }
}
