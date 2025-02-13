using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.UserActivityLogModels;
using Chillde.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/logs")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<LogController> _logger;

        public LogController(IRabbitMQService rabbitMQService, ILogger<LogController> logger)
        {
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostLog([FromBody] UserActivityLogAddModel log)
        {
            try
            {
                _logger.LogInformation("Received log: {@Log}", log);
                await _rabbitMQService.SendMessageAsync("user_activity_queue", log);
                return Ok(new { message = "Log received and sent to queue." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process log");
                return StatusCode(500, new { message = "Failed to process log", error = ex.Message });
            }
        }
    }
}