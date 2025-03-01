using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.Services.Interfaces
{
    public interface IShipmentService
    {
        Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel requestModel);
        Task<CancelShipmentResponseModel> CancelShipmentAsync(string trackingOrder);
        Task<byte[]> GetShippingLabelAsync(string trackingOrder);
        Task<ResponseModel> GetOrderStatusAsync(string trackingOrder);
        Task<ResponseModel> CreateShipmentAsync(ShipmentCreateModel shipmentCreateModel);

    }
}
