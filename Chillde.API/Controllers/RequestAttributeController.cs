using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/requests")]
    [ApiController]
    public class RequestAttributeController : ControllerBase
    {
        private readonly IRequestAttributeService _requestAttributeService;

        public RequestAttributeController(IRequestAttributeService requestAttributeService)
        {
            _requestAttributeService = requestAttributeService;
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAttribute([FromRoute] Guid id)
        {
            try
            {

                var result = await _requestAttributeService.RemoveAttribute(id);
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
        [HttpDelete("{attributeValueId}/attribute-values")]
        public async Task<IActionResult> RemoveAttributeValue([FromRoute] Guid attributeValueId)
        {
            try
            {

                var result = await _requestAttributeService.RemoveAttributeValue(attributeValueId);
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
    }
}
