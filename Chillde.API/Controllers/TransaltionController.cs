using Chillde.Services.Interfaces;
using Chillde.Services.Models.TranslationModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Chillde.Repositories.Enums;
using Chillde.API.Helper;

namespace Chillde.API.Controllers
{
    [Route("api/v1/translations")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TransaltionAddModel model)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var translationResult = await _translationService.TranslateAsync(model.TranslationText, sourceLanguageCode, targetLanguageCode);
                if (translationResult.Code != StatusCodes.Status200OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to translate text."
                    });
                }
                model.TranslationText = translationResult.Message;

                var saveResult = await _translationService.SaveTranslationAsync(model, targetLanguageCode.ToLower());
                if (saveResult.Code == StatusCodes.Status200OK)
                {
                    return Ok(saveResult);
                }
                return BadRequest(saveResult);
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

        //[Authorize]
        [HttpPut()]
        public async Task<IActionResult> Update([FromBody] TransaltionAddModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.EntityType) || model.EntityId == Guid.Empty || string.IsNullOrEmpty(model.FieldName))
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid data provided."
                    });
                }

                var targetLanguageCode = Request.Headers["Accept-Language"].ToString();
                if (!Enum.TryParse<LanguageCode>(targetLanguageCode, true, out var languageCode))
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid language code provided."
                    });
                }

                var result = await _translationService.UpdateAsync(model, languageCode);

                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return NotFound(result);
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
        [HttpDelete()]
        public async Task<IActionResult> Delete(string entityType, Guid entityId, string fieldName, Guid languageId)
        {
            try
            {
                var result = await _translationService.DeleteAsync(entityType, entityId, fieldName, languageId);

                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return NotFound(result);
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
