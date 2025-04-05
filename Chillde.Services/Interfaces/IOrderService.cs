using Chillde.Repositories.Enums;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.OrderTrackingModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel> BalancePayment(OrderAddModel orderAddModel);
        Task<ResponseModel> CreatePaymentUrl(OrderAddModel orderAddModel, string ipAddress);
        Task<ResponseModel> UpdateOrderStatusToCompleted(Guid orderId);
        Task<ResponseModel> UsedAdminVoucher(Guid orderId, Guid voucherId);
        Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel, Guid orderId);
        Task<ResponseModel> CancelShipmentAsync(Guid orderId, string shipmentCode);
        Task<ResponseModel> GetAll(OrderFilterModel orderFilterModel);
        Task<ResponseModel> UpdateStatus(Guid orderId, OrderStatus? orderStatus);
        Task<ResponseModel> Cancel(Guid orderId, Guid cancellationReasonId);
        Task<ResponseModel> GetAllOrderTrackings(Guid orderId, OrderStage? orderStage);
        Task<ResponseModel> AddSketch(Guid orderId, OrderTrackingAddModel orderTrackingAddModel);
        Task<ResponseModel> GetAllByAdmin(OrderFilterModel orderFilterModel);
        Task<ResponseModel> AddDelivery(Guid orderId, OrderTrackingAddModel orderTrackingAddModel);

    }
}
