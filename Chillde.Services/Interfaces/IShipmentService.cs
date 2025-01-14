using Chillde.Repositories.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;

namespace Chillde.Services.Interfaces
{
    public interface IShipmentService
    {
        Task<ResponseModel> AddShipmentAsync(ShipmentAddModel model);
        Task<ResponseModel> CalculateShippingFeeAsync(ShippingFeeRequestModel? requestModel);
    }
}
