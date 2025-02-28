using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IFeatureService
    {
        Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> UpdateAsync(FeatureUpdateModel featureUpdateModel, Guid id);
    }
}
