using Chillde.Services.Interfaces;
using Chillde.Services.Models.BadWordFilterModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/bad-word-filter")]
    [ApiController]
    public class BadWordFilterController : ControllerBase
    {
        private readonly IBadWordFilterService _badWordFilterService;

        public BadWordFilterController(IBadWordFilterService badWordFilterService)
        {
            _badWordFilterService = badWordFilterService;
        }

        //[Authorize("Artist")]
        [HttpPost]
        public async Task<IActionResult> FilterBadWordAsync([FromBody] BadWordFilterModel serviceAddModel)
        {
            try
            {
                var result = await _badWordFilterService.FilterBadWordsAsync(serviceAddModel);
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
