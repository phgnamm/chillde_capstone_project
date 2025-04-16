using Chillde.API.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.ReputationLogModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/reputation-log")]
    [ApiController]
    public class ReputationLogController : ControllerBase
    {
        private readonly IReputationLogService _reputationLogService;

        public ReputationLogController(IReputationLogService reputationLogService)
        {
            _reputationLogService = reputationLogService;
        }

        //[Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetAll([FromQuery] ReputationLogFilterModel reputationLogFilterModel)
        {
            try
            {
                var result = await _reputationLogService.GetAll(reputationLogFilterModel);
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
