using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Models.VoucherUsageLogModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/voucher-usages")]
    [ApiController]
    public class VoucherUsageLogController : ControllerBase
    {
        private readonly IVoucherUsageLogService _voucherUsageLogService;

        public VoucherUsageLogController(IVoucherUsageLogService voucherUsageLogService)
        {
            _voucherUsageLogService = voucherUsageLogService;
        }

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VoucherUsageLogFilterModel voucherUsageLogFilterModel)
        {
            try
            {
                var result = await _voucherUsageLogService.GetAll(voucherUsageLogFilterModel);
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
