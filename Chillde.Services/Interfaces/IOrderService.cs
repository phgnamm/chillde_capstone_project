using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseModel> BalancePayment(OrderAddModel orderAddModel);
        Task<ResponseModel> CreatePaymentUrl(OrderAddModel orderAddModel, string ipAddress);
        Task<ResponseModel> UpdateOrderStatusToCompleted(Guid orderId);

    }
}
