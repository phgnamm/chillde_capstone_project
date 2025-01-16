using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;

namespace Chillde.Services.Interfaces
{
    public interface IShipmentService
    {
        Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel? requestModel);
        Task<ResponseModel> GetShipmentDetailAsync(string orderCode);
        Task<ResponseModel> SwitchToReturnStatusAsync(string orderCode);

    }
}
