using Chillde.Repositories.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IPackageService
    {
        Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id);
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, Guid packageId);
    }
}
