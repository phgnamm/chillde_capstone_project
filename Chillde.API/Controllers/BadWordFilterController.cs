using Chillde.API.Helper;
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
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _badWordFilterService.FilterBadWordsAsync(serviceAddModel, sourceLanguageCode, targetLanguageCode);
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
