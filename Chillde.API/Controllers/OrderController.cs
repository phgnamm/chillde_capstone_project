using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShipmentModels;
using Chillde.Services.Services;
using Chillde.Services.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace Chillde.API.Controllers
{
    [Route("api/v1/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;

        public OrderController(IOrderService orderService, IVnpay vnpay, IConfiguration configuration)
        {
            _orderService = orderService;
            _vnpay = vnpay;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);

        }
        [Authorize]
        [HttpGet("artisans")]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilterModel orderFilterModel)
        {
            try
            {
                var result = await _orderService.GetAll(orderFilterModel);
                if (result.Status)
                {
                    return Ok(result);
                }
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
        [Authorize]
        [HttpPut("artisans")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] OrderStatus orderStatus)
        {
            try
            {
                var result = await _orderService.UpdateStatus(orderId, orderStatus);
                if (result.Status)
                {
                    return Ok(result);
                }
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
        [Authorize]
        [HttpPost("create-payment-url")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] OrderAddModel order)
        {
            try
            {
                string ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var result = await _orderService.CreatePaymentUrl(order, ipAddress);

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
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = await _vnpay.GetPaymentResult(Request.Query);

                    if (paymentResult.IsSuccess)
                    {
                        var orderId = paymentResult.OrderId; 

                        var updateResult = await _orderService.UpdateOrderStatusToCompleted(orderId);

                        if (updateResult.Code == StatusCodes.Status200OK)
                        {
                            return Ok(new
                            {
                                Message = "Payment and order update successful.",
                                PaymentResult = paymentResult,
                                OrderUpdateResult = updateResult
                            });
                        }

                        return BadRequest(new
                        {
                            Message = "Payment successful but order update failed.",
                            PaymentResult = paymentResult,
                            OrderUpdateResult = updateResult
                        });
                    }
                    return BadRequest(new
                    {
                        Message = "Payment failed.",
                        PaymentResult = paymentResult
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        Message = "An error occurred during payment callback.",
                        Error = ex.Message
                    });
                }
            }

            return NotFound(new
            {
                Message = "No payment information found in the request."
            });
        }

        //[Authorize("Artisan")]
        [HttpPost("{orderId}/shipments")]
        public async Task<IActionResult> AddShipmentAsync([FromBody] ShipmentAddModel model, Guid orderId)
        {
            try
            {
                var result = await _orderService.CreateShipmentAsync(model, orderId);
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
        //[Authorize("Artisan")]
        [HttpPost("{orderId}/shipments/{shipmentCode}")]
        public async Task<IActionResult> CancelShipmentAsync(Guid orderId, string shipmentCode)
        {
            try
            {
                var result = await _orderService.CancelShipmentAsync(orderId, shipmentCode);
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
