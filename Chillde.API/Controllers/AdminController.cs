using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.OrderModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/admins")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IOrderService _orderService;

        public AdminController(IAccountService accountService, IOrderService orderService)
        {
            _accountService = accountService;
            _orderService = orderService;
        }
        [HttpPost("ban-role")]
        public async Task<IActionResult> BanAccountRole([FromQuery] BanAccountRoleModel request)
        {
            try
            {
                var result = await _accountService.BanAccountRole(request);
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
        [HttpGet("orders")]
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
        }
    }
}
