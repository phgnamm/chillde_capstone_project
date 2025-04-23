using Chillde.Repositories.Models.NotificationModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.NotificationModels;
using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> AddMessage([FromBody] NotificationAddModel notificationAddModel)
        {
            try
            {
                var result = await _notificationService.PushNotification(notificationAddModel);
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
        public async Task<IActionResult> GetAll([FromQuery] NotificationFilterModel notificationFilterModel)
        {
            try
            {
                var result = await _notificationService.GetAll(notificationFilterModel);
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
