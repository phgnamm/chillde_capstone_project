using Chillde.Services.Models.BadWordFilterModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IBadWordFilterService
    {
        Task<ResponseModel> FilterEnglishBadWordsAsync(string content);
        Task<ResponseModel> FilterVietnameseBadWordsAsync(string content);
    }
}
