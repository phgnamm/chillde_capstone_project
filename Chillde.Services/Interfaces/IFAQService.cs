using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IFAQService
    {
        Task<ResponseModel> UpdateAsync(FAQAddAndUpdateModel faqAddAndUpdateModel, Guid id, string sourceLanguageCode);
        Task<ResponseModel> HardDeleteAsync(Guid id);
    }
}
