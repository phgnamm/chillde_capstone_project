using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/service-attachments")]
    [ApiController]
    public class ServiceAttachmentController : ControllerBase
    {
        private readonly IServiceAttachmentService _serviceAttachmentService;

        public ServiceAttachmentController(IServiceAttachmentService serviceAttachmentService)
        {
            _serviceAttachmentService = serviceAttachmentService;
        }

        //[Authorize("Artisan, Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteServiceAttachmentAsync([FromBody] List<Guid> serviceAttacchmentIds)
        {
            try
            {
                var result = await _serviceAttachmentService.DeleteServiceAttachmentAsync(serviceAttacchmentIds);
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
