using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Interfaces;
using Chillde.Services.Models;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SystemConfigModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/system-configs")]
    [ApiController]
    public class SystemConfigController : ControllerBase
    {
        private readonly ISystemConfigService _systemConfigService;

        public SystemConfigController(ISystemConfigService systemConfigService)
        {
            _systemConfigService = systemConfigService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SystemConfigAddModel model)
        {
            try
            {
                var result = await _systemConfigService.Add(model);
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

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SystemConfigAddModel model)
        {
            try
            {
                var result = await _systemConfigService.Update(model);
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


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SystemConfigFilterModel model)
        {
            try
            {
                var result = await _systemConfigService.GetAll(model);
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
