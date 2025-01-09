using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SuggestModels;
namespace Chillde.Services.Interfaces
{
    public interface IOpenAiService
    {
        Task<ResponseModel> GetRecommendationsAsync(SuggestAddModel suggestAddModel);
    }
}
