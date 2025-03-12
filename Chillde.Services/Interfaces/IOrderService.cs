using Chillde.Repositories.Enums;
using Chillde.Services.Models.OrderModels;
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
        Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel, Guid orderId);
        Task<ResponseModel> CancelShipmentAsync(Guid orderId, string shipmentCode);
        Task<ResponseModel> GetAll(OrderFilterModel orderFilterModel);
        Task<ResponseModel> UpdateStatus(Guid orderId, OrderStatus orderStatus);
    }
}
