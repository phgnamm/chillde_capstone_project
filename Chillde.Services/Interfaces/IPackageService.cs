using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IPackageService
    {
        Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, Guid packageId, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> GetAllFeatureAsync(FeatureFilterModel model, Guid packageId, string sourceLanguageCode, string targetLanguage);
    }
}
