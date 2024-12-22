using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<ResponseModel> Add(FeedbackAddModel feedbackAddModel);
        Task<ResponseModel> GetAllByService(Guid serviceId, FeedbackFilterModel feedbackFilterModel);
        Task<ResponseModel> GetAllByServiceAndUser(Guid serviceId, FeedbackFilterModel feedbackFilterModel);
        Task<ResponseModel> Update(Guid id, FeedbackUpdateModel feedbackUpdateModel);
        Task<ResponseModel> GetById(Guid id);
    }
}
