using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.OrderTrackingModels;
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
        private readonly IOrderTrackingService _orderTrackingService;
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;

        public OrderController(IOrderTrackingService orderTrackingService, IOrderService orderService, IVnpay vnpay, IConfiguration configuration)
        {
            _orderTrackingService = orderTrackingService;
            _orderService = orderService;
            _vnpay = vnpay;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);

        }
        [Authorize]
        [HttpGet]
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
       /* [HttpGet]
        public async Task<IActionResult> GetAllByAdmin([FromQuery] OrderFilterModel orderFilterModel)
        {
            try
            {
                var result = await _orderService.GetAllByAdmin(orderFilterModel);
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
        }*/
        [Authorize]
        [HttpPost("use-admin-vouchers")]
        public async Task<IActionResult> UsedAdminVoucher(Guid orderId, Guid voucherId)
        {
            try
            {
                var result = await _orderService.UsedAdminVoucher(orderId, voucherId);
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
        [HttpPut("order-accepts")]
        public async Task<IActionResult> UpdateStatus(Guid orderId, OrderStatus orderStatus)
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
                    Message = ex.ToString()
                });
            }
        }
        [Authorize]
        [HttpPost("balance-payment")]
        public async Task<IActionResult> BalancePayment([FromBody] OrderAddModel order)
        {
            try
            {
                var result = await _orderService.BalancePayment(order);

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
        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
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
                        return Ok();
                    }

                    // Thực hiện hành động nếu thanh toán thất bại tại đây. Ví dụ: Hủy đơn hàng.
                    return BadRequest("Thanh toán thất bại");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
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
        public async Task<IActionResult> AddShipmentAsync([FromBody] ShipmentCreateModel model, Guid orderId)
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
        [Authorize]
        [HttpPost("{orderId}/order-tracking-sketches")]
        public async Task<IActionResult> AddSketch(Guid orderId, [FromForm] OrderTrackingAddModel orderTrackingAddModel)
        {
            try
            {
                var result = await _orderService.AddSketch(orderId, orderTrackingAddModel);
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
        [HttpPost("{orderId}/order-tracking-deliveries")]
        public async Task<IActionResult> AddDelivery(Guid orderId, [FromForm] OrderTrackingAddModel orderTrackingAddModel)
        {
            try
            {
                var result = await _orderService.AddDelivery(orderId, orderTrackingAddModel);
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
        [HttpPut("{orderId}/cancel-order/{cancellationReasonId}")]
        public async Task<IActionResult> Cancel(Guid orderId, Guid cancellationReasonId)
        {
            try
            {
                var result = await _orderService.Cancel(orderId, cancellationReasonId);
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
        [HttpGet("{orderId}/get-order-trackings")]
        public async Task<IActionResult> GetAllOrderTrackings(Guid orderId, OrderStage? orderStage)
        {
            try
            {
                var result = await _orderService.GetAllOrderTrackings(orderId, orderStage);
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
        [HttpGet("{orderId}/details")]
        public async Task<IActionResult> GetOrderDetail(Guid orderId)
        {
            try
            {
                var result = await _orderService.GetOrderDetail(orderId);
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
