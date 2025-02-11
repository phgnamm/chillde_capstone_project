using Chillde.API.Helper;
using Chillde.Repositories.Enums;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Models.SuggestModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/services")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IOpenAiService _openAiService;

        public ServiceController(IServiceService serviceService, IOpenAiService openAiService)
        {
            _serviceService = serviceService;
            _openAiService = openAiService;
        }

        //[Authorize]
        [HttpPost("feedbacks")]
        public async Task<IActionResult> AddFeedback([FromForm] FeedbackAddModel feedbackAddModel)
        {
            try
            {
                var result = await _serviceService.AddFeedbackAsync(feedbackAddModel);
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
        [HttpGet("{id}/feedbacks")]
        public async Task<IActionResult> GetAllFeedbacksByService(Guid id, [FromQuery] FeedbackFilterModel feedbackFilterModel)
        {
            try
            {
                var result = await _serviceService.GetAllFeedbacksByServiceAsync(id, feedbackFilterModel);
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
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ServiceFilterModel serviceFilterModel)
        {
            try
            {
                var result = await _serviceService.Search(serviceFilterModel);
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
        public async Task<IActionResult> GetAll([FromQuery] ServiceFilterModel serviceFilterModel)
        {
            try
            {
                var result = await _serviceService.GetAll(serviceFilterModel);
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
        [HttpGet("{id}/feedbacks-users")]
        public async Task<IActionResult> GetAllByServiceAndUser(Guid id, [FromQuery] FeedbackFilterModel feedbackFilterModel)
        {
            try
            {
                var result = await _serviceService.GetAllFeedbacksByServiceAndUserAsync(id, feedbackFilterModel);
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
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _serviceService.GetAsync(id);
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

        //[Authorize("Artist")]
        [HttpPost]
        public async Task<IActionResult> AddServiceAsync([FromForm] ServiceAddModel serviceAddModel)
        {
            try
            {
                var result = await _serviceService.AddAsync(serviceAddModel);
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

        //[Authorize("Artist, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ServiceUpdateModel serviceUpdateModel, Guid id)
        {
            try
            {
                var result = await _serviceService.UpdateAsync(serviceUpdateModel, id);
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

        //[Authorize("Artist, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _serviceService.DeleteAsync(id);
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
        [HttpGet("{serviceId}/service-attachments")]
        public async Task<IActionResult> GetAllServiceAttachmentsAsync(Guid serviceId)
        {
            try
            {
                var result = await _serviceService.GetServiceAttachmentssAsync(serviceId);
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
        [HttpGet("{serviceId}/packages")]
        public async Task<IActionResult> GetAllPackagesAsync(Guid serviceId, [FromQuery] PackageFilterModel packageFilterModel)
        {
            try
            {
                var result = await _serviceService.GetAllPackagesAsync(packageFilterModel, serviceId);
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

        //[Authorize("Artist")]
        [HttpPost("{serviceId}/packages")]
        public async Task<IActionResult> AddPackageAsync([FromBody] PackageAddModel packageAddModel, Guid serviceId)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _serviceService.AddPackageAsync(packageAddModel, serviceId, sourceLanguageCode, targetLanguageCode);
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

        //[Authorize("Artist")]
        [HttpPost("{serviceId}/faqs")]
        public async Task<IActionResult> AddFAQAsync([FromBody] FAQAddAndUpdateModel faqAddModel, Guid serviceId)
        {
            try
            {
                var result = await _serviceService.AddFAQAsync(faqAddModel, serviceId);
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
        [HttpGet("{serviceId}/faqs")]
        public async Task<IActionResult> GetAllFAQsAsync(Guid serviceId, [FromQuery]FAQFilterModel faqFilterModel)
        {
            try
            {
                var result = await _serviceService.GetAllFAQsAsync(serviceId, faqFilterModel);
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
        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] SuggestAddModel suggestAddModel)
        {
            try
            {
                var result = await _openAiService.GetRecommendationsAsync(suggestAddModel);
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
