using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceWishlistModels;

namespace Chillde.Services.Interfaces
{
    public interface IServiceWishlistService
    {
        Task<ResponseModel> AddRangeAsync(Guid serviceCollectionId, List<Guid> serviceIds);
        Task<ResponseModel> GetAllAsync(Guid ServiceCollectionId, ServiceWishlistFilterModel filterModel);
        Task<ResponseModel> GetByIdAsync(Guid id);
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> UpdateAsync(Guid id, Guid newServiceId);
    }
}
