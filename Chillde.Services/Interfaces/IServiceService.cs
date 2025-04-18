using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Services.Helpers;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceAttachmentModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Interfaces
{
    public interface IServiceService
    {
        Task<ResponseModel> AddFeedbackAsync(FeedbackAddModel feedbackAddModel, string sourceLanguageCode);
        Task<ResponseModel> GetAllFeedbacksByServiceAndUserAsync(Guid serviceId, FeedbackFilterModel feedbackFilterModel);
        Task<ResponseModel> GetAllFeedbacksByServiceAsync(Guid serviceId, FeedbackFilterModel feedbackFilterModel);
        Task<ResponseModel> Search(ServiceFilterModel serviceFilterModel);
        Task<ResponseModel> GetAll(ServiceFilterModel serviceFilterModel);
        Task<ResponseModel> GetAllPackagesByServiceAsync(PackageFilterModel packageFilterModel, Guid serviceId);
        Task<ResponseModel> GetAllFeaturesByServiceAsync(Guid serviceId);
        Task<ResponseModel> AddPackageAsync(PackageAddModel packageAddModel, Guid serviceId,string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> GetAsync(Guid id);
        Task<ResponseModel> AddAsync(ServiceAddModel serviceAddModel, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> UpdateAsync(ServiceUpdateModel serviceUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> ActiveAsync(Guid id);
        Task<ResponseModel> GetServiceAttachmentssAsync(Guid serviceId);
        Task<ResponseModel> AddListServiceAttachmentAsync(List<AttachmentAddModel> attachmentModels, Guid serviceId);
        Task<ResponseModel> AddFAQAsync(FAQAddAndUpdateModel faqAddModel, Guid serviceId, string sourceLanguageCode);
        Task<ResponseModel> GetAllFAQsAsync(Guid serviceId, FAQFilterModel faqFilterModel);
        Task<ResponseModel> GetAllWithSuggestion(ServiceFilterModel serviceFilterModel, string sourceLanguageCode, string targetLanguageCode);
    }
}
