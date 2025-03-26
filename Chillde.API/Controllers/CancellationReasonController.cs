using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CancellationReasonModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/cancellation-reasons")]
    [ApiController]
    public class CancellationReasonController : ControllerBase
    {
        private readonly ICancellationReasonService _cancellationReasonService;

        public CancellationReasonController(ICancellationReasonService cancellationReasonService)
        {
            _cancellationReasonService = cancellationReasonService;
        }
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] CancellationReasonAddModel cancellationReasonAddModel)
        {
            try
            {
                var result = await _cancellationReasonService.AddAsync(cancellationReasonAddModel);
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
        public async Task<IActionResult> Delete([FromQuery] Guid id)
        {
            try
            {
                var result = await _cancellationReasonService.DeleteAsync(id);
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
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _cancellationReasonService.GetAllAsync();
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
        [HttpPut("update")]
        public async Task<IActionResult> Add([FromQuery] Guid id, [FromBody] CancellationReasonAddModel cancellationReasonAddModel)
        {
            try
            {
                var result = await _cancellationReasonService.UpdateAsync(id, cancellationReasonAddModel);
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
