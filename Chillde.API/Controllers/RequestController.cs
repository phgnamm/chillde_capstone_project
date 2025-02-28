using Chillde.API.Helper;
using Chillde.Repositories.Enums;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/requests")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IOfferService _offerService;
        private readonly IOpenAiService _openAiService;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public RequestController(IRequestService requestService, IOfferService offerService, IOpenAiService openAiService, ICloudinaryHelper cloudinaryHelper)
        {
            _requestService = requestService;
            _offerService = offerService;
            _openAiService = openAiService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RequestFilterModel requestFilterModel)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _requestService.GetAll(requestFilterModel,sourceLanguageCode, targetLanguageCode);
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] RequestAddModel requestAddModel)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _requestService.Add(requestAddModel,sourceLanguageCode,targetLanguageCode);
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
        [Authorize]
        [HttpPost("test")]
        public async Task<IActionResult> AddAsync([FromForm] RequestAddModel requestAddModel)
        {
            try
            {

                var result = await _requestService.AddAsync(requestAddModel);
                if (result.Status)
                {
                    return Ok(result);
                }
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

        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] RequestUpdateModel requestUpdateModel)
        {
            try
            {
                //var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                //var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                //var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _requestService.UpdateRequestAsync(id, requestUpdateModel);
                if (result.Status)
                {
                    return Ok(result);
                }
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
        //[Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                //var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                //var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                //var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _requestService.GetByIdAsync(id);
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

        [HttpPost("{requestId}/offers")]
        public async Task<IActionResult> Add([FromBody] OfferAddModel model, Guid requestId)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _offerService.AddAsync(model, requestId, sourceLanguageCode, targetLanguageCode);
                if (result.Code != StatusCodes.Status201Created)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = result.Message
                    });
                }
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
        [HttpPost("ai-generate-forms")]
        public async Task<IActionResult> AiGenerateForm(List<string> attributes)
        {
            try
            {
               
                var result = await _openAiService.GetStructuredDataAsync(attributes);
                if (result.Status)
                {
                    return Ok(result);
                }
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

        [HttpGet("{requestId}/offers")]
        public async Task<IActionResult> GetAll([FromQuery] OfferFilterModel filterParameter, Guid requestId)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _offerService.GetAllAsync(filterParameter, requestId, sourceLanguageCode, targetLanguageCode);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = result.Message
                    });
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
