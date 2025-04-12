using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IPackageService
    {
        Task<ResponseModel> GetAsync(Guid id);
        Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> GetAllFeatureByPackageAsync(FeatureFilterModel model, Guid packageId, string sourceLanguageCode, string targetLanguage);
        Task<ResponseModel> AddPackageFeatureAsync(PackageFeatureAddModel packageFeatureAddModel, Guid packageId, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> AddRangePackageFeatureAsync(List<PackageFeatureAddModel> packageFeatureAddModels, Guid packageId, string sourceLanguageCode, string targetLanguageCode);
    }
}
