using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Models.ShipmentModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/shipments")]
    [ApiController]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        //[Authorize]
        [HttpPost("calculate-fee")]
        public async Task<IActionResult>CalculateFee([FromBody] ShippingFeeRequestModel shippingFeeRequestModel)
        {
            try
            {
                var result = await _shipmentService.CalculateShippingFeeAsync(shippingFeeRequestModel);
                return StatusCode(result.Code, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }

        
    }
}
