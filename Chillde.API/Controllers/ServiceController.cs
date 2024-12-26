using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
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

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] FeedbackAddModel feedbackAddModel)
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
        [Authorize]
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

        [Authorize]
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

        [Authorize("Artist")]
        [HttpPost("{serviceId}/packages")]
        public async Task<IActionResult> AddPackageAsync([FromBody] PackageAddModel packageAddModel, Guid serviceId)
        {
            try
            {
                var result = await _serviceService.AddPackageAsync(packageAddModel, serviceId);
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

        [Authorize("Artist")]
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

        [Authorize]
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
    }
}
