using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.VnPayModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Chillde.Services.Utils;
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

        [HttpGet("createPaymentUrl")]
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
                        return Ok(paymentResult);
                    }

                    return BadRequest(paymentResult);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}
