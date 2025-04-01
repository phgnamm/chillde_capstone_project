using Chillde.Repositories.Enums;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/order-trackings")]
    [ApiController]
    public class OrderTrackingController : ControllerBase
    {
        private readonly IOrderTrackingService _orderTrackingService;

        public OrderTrackingController(IOrderTrackingService orderTrackingService)
        {
            _orderTrackingService = orderTrackingService;
        }
        [Authorize]
        [HttpPut("change-accepted")]
        public async Task<IActionResult> UpdateStatus(Guid orderTrackingId, bool isAccept)
        {
            try
            {
                var result = await _orderTrackingService.ChangeAccepted(orderTrackingId, isAccept);
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
    }
}
