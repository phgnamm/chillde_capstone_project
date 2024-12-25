using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IServiceService
    {
        Task<ResponseModel> AddPackageAsync(PackageAddModel packageAddModel, Guid serviceId);
        Task<ResponseModel> GetAsync(Guid id);
        Task<ResponseModel> GetServiceAttachmentssAsync(Guid serviceId);
        Task<ResponseModel> AddFAQAsync(FAQAddAndUpdateModel faqAddModel, Guid serviceId);
        Task<ResponseModel> GetAllFAQsAsync(Guid serviceId, FAQFilterModel faqFilterModel);
    }
}
