using Chillde.Repositories.Enums;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
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
        public async Task<IActionResult> CalculateFee([FromBody] ShippingFeeRequestModel shippingFeeRequestModel)
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
        //[Authorize]
        [HttpGet("cancel/{trackingCode}")]
        public async Task<IActionResult> CancelShipmentAsync(string trackingCode)
        {
            try
            {
                var result = await _shipmentService.CancelShipmentAsync(trackingCode);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
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
        //[Authorize]
        [HttpGet("print-label/{trackingOrder}")]
        public async Task<IActionResult> PrintShippingLabel(string trackingOrder)
        {
            try
            {
                var pdfBytes = await _shipmentService.GetShippingLabelAsync(trackingOrder);
                return new FileContentResult(pdfBytes, "application/pdf")
                {
                    FileDownloadName = $"shipping-label-{trackingOrder}.pdf",
                    EnableRangeProcessing = true 
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpGet("status/{trackingOrder}")]
        public async Task<IActionResult> GetOrderStatus(string trackingOrder)
        {
            try
            {
                var response = await _shipmentService.GetOrderStatusAsync(trackingOrder);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        ////[Authorize]
        //[HttpPost("create-shipment")]
        //public async Task<IActionResult> CreateShipment([FromBody] ShipmentCreateModel model)
        //{
        //    try
        //    {
        //        var result = await _shipmentService.CreateShipmentAsync(model);
        //        return StatusCode(result.Code, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = ex.Message
        //        });
        //    }
        //}
        [HttpPut("{shipmentId}/status")]
        public async Task<IActionResult> UpdateShipmentStatus(Guid shipmentId, [FromBody] ShipmentStatus newStatus)
        {
            var response = await _shipmentService.UpdateShipmentStatusAsync(shipmentId, newStatus);
            return StatusCode(response.Code, response);
        }

    }
}
