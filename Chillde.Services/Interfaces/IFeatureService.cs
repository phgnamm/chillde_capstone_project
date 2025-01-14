using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IFeatureService
    {
        Task<ResponseModel> UpdateAsync(FeatureUpdateModel featureUpdateModel, Guid id);
    }
}
