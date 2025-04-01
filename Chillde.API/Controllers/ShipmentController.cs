using Chillde.Repositories.Enums;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
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
        //[Authorize]
        [HttpPost("webhook/shipment-update")]
        public async Task<IActionResult> UpdateShipment([FromForm] ShipmentUpdateRequestModel request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.LabelId) || request.StatusId == 0)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                var result = await _shipmentService.UpdateShipmentStatusAsync(request);

                if (result)
                {
                    return Ok(new { message = "Cập nhật thành công" });
                }
                else
                {
                    return StatusCode(500, new { message = "Cập nhật thất bại" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý webhook", error = ex.Message });
            }
        }

    }
}
