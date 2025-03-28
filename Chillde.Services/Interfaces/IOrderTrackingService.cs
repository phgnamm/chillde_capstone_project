using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;
using Chillde.Services.Models.OrderTrackingModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IOrderTrackingService
    {
        Task<ResponseModel> GetAll(Guid orderId, OrderStage? orderStage);
        Task<ResponseModel> Add(Guid orderId, OrderTrackingAddModel orderTrackingAddModel);
        Task<ResponseModel> Accepted(Guid orderTrackingId);
        Task<ResponseModel> Cancel(Guid orderId, Guid cancellationReasonId);
        Task<ResponseModel> SendReminder(Guid orderTrackingId);
    }
}
