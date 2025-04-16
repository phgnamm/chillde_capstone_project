using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IRequestService
    {
        //Task<ResponseModel> Add(RequestAddModel requestAddModel);
        Task<ResponseModel> UpdateRequestAsync(Guid requestId, RequestUpdateModel requestUpdateModel);
        //Task<ResponseModel> GetById(Guid id, string sourceLanguageCode, string targetLanguage);
        Task<ResponseModel> GetByIdAsync(Guid id);
        //Task<ResponseModel> GetAll(RequestFilterModel requestFilterModel);
        Task<ResponseModel> GetAll(RequestFilterModel filterParameter, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> Get(RequestFilterModel filterParameter);
        Task<ResponseModel> Add(RequestAddModel requestAddModel, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> AddAsync(RequestAddModel requestAddModel, string sourceLanguageCode);
        Task<ResponseModel> DeleteAsync(Guid id);
    }
}
