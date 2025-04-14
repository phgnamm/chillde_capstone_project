using Chillde.Services.Interfaces;
using Chillde.Services.Models.DepositModels;
using Chillde.Services.Models.ReputationLogModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/deposits")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        private readonly IDepositService _depositService;

        public DepositController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        //[Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetAll([FromQuery] DepositFilterModel depositFilterModel)
        {
            try
            {
                var result = await _depositService.GetAll(depositFilterModel);
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
