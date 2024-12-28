using Chillde.Services.Models.ConversationModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IRequestService
    {
        //Task<ResponseModel> Add(RequestAddModel requestAddModel);
        Task<ResponseModel> Update(Guid id, RequestUpdateModel requestAddModel);
        Task<ResponseModel> GetById(Guid id);
        //Task<ResponseModel> GetAll(RequestFilterModel requestFilterModel);
        Task<ResponseModel> GetAll(RequestFilterModel filterParameter, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> Add(RequestAddModel requestAddModel, string sourceLanguageCode, string targetLanguageCode);
    }
}
