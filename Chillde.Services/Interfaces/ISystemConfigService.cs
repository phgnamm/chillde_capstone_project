using Chillde.Repositories.Enums;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SystemConfigModels;

namespace Chillde.Services.Interfaces
{
    public interface ISystemConfigService
    {
        Task<ResponseModel> Add(SystemConfigAddModel model);
        Task<ResponseModel> Update(Guid id, SystemConfigUpdateModel model);
        Task<ResponseModel> GetAll(SystemConfigFilterModel model);
        Task<ResponseModel> Get(SystemConfigKey key);
        Task<List<object[]>> GetAllAsKeyValueAsync();
    }
}
