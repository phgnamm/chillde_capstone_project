using Chillde.Repositories.Enums;
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

        //[Authorize("Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] SystemConfigUpdateModel model)
        {
            try
            {
                var result = await _systemConfigService.Update(id, model);
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

        [HttpGet("key")]
        public async Task<IActionResult> Get([FromQuery] SystemConfigKey key)
        {
            try
            {
                var result = await _systemConfigService.Get(key);
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

        [HttpGet("enum-list")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _systemConfigService.GetAllAsKeyValueAsync();
                return StatusCode(StatusCodes.Status200OK, new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = result
                });
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
