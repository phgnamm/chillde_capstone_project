using Chillde.Services.Models.BadWordFilterModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IBadWordFilterService
    {
        Task<ResponseModel> FilterBadWordsAsync(BadWordFilterModel badWordFilterModel, string sourceLanguageCode, string targetLanguageCode);
    }
}
