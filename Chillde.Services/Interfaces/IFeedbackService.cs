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
        Task<ResponseModel> Update(Guid id, FeedbackUpdateModel feedbackUpdateModel);
        Task<ResponseModel> GetById(Guid id);
        Task<ResponseModel> RespondToFeedback(Guid feedbackId, string responseText);
        Task<ResponseModel> GetAllFeedbacksByArtisanAsync(Guid accountId, FeedbackFilterModel feedbackFilterModel);
    }
}
