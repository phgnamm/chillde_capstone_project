

using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceCollectionModels;

namespace Chillde.Services.Interfaces
{
    public interface IServiceCollectionService
    {
        Task<ResponseModel> AddAsync(ServiceCollectionAddModel model);
        Task<ResponseModel> GetAllAsync(ServiceCollectionFilterModel filterModel);
        Task<ResponseModel> GetByIdAsync(Guid id);
        Task<ResponseModel> UpdateAsync(Guid id, ServiceCollectionAddModel model);
        Task<ResponseModel> DeleteAsync(Guid id);
    }
}
