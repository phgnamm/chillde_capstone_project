using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService  _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeedbackUpdateModel feedbackUpdateModel)
        {
            try
            {
                var result = await _feedbackService.Update(id, feedbackUpdateModel);
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
                var result = await _feedbackService.GetById(id);
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
        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToFeedback(Guid id, [FromBody] string responseText)
        {
            try
            {
                var result = await _feedbackService.RespondToFeedback(id, responseText);
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
        [HttpGet("artisan/{accountId}/feedbacks")]
        public async Task<IActionResult> GetAllFeedbacksByArtisan(Guid accountId, [FromQuery] FeedbackFilterModel filter)
        {
            try
            {
                var result = await _feedbackService.GetAllFeedbacksByArtisanAsync(accountId,filter);
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
