using Chillde.Services.Interfaces;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Chillde.API.Helper;

namespace Chillde.API.Controllers
{
    [Route("api/v1/offers")]
    [ApiController]
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OfferController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        // [Authorize]
        [HttpPut("{offerId}")]
        public async Task<IActionResult> Update(Guid offerId, [FromBody] OfferUpdateModel model)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _offerService.UpdateAsync(offerId, model, sourceLanguageCode, targetLanguageCode);
                if (result.Code == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }
                return Ok(result);
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

        // [Authorize]
        [HttpDelete("{offerId}")]
        public async Task<IActionResult> Delete(Guid offerId)
        {
            try
            {
                var result = await _offerService.DeleteAsync(offerId);
                if (result.Code == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }
                return Ok(result);
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
