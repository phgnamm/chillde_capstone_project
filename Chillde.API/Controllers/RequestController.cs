using Chillde.API.Helper;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ConversationModels;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/requests")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IOfferService _offerService;
        public RequestController(IRequestService requestService, IOfferService offerService)
        {
            _requestService = requestService;
            _offerService = offerService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RequestFilterModel requestFilterModel)
        {
            try
            {
                var result = await _requestService.GetAll(requestFilterModel);
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
                var result = await _requestService.Add(requestAddModel);
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] RequestUpdateModel requestUpdateModel)
        {
            try
            {
                var result = await _requestService.Update(id, requestUpdateModel);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _requestService.GetById(id);
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

        [HttpPost("{request_id}/Offer")]
        public async Task<IActionResult> Add([FromBody] OfferAddModel model, Guid request_id)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _offerService.AddAsync(model, request_id, sourceLanguageCode, targetLanguageCode);
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

        [HttpGet("{request_id}/Offers")]
        public async Task<IActionResult> GetAll([FromQuery] OfferFilterModel filterParameter, Guid request_id)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);

                var result = await _offerService.GetAllAsync(filterParameter, request_id, sourceLanguageCode, targetLanguageCode);
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
