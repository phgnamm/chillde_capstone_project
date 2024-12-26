using Chillde.Services.Interfaces;
using Chillde.Services.Models.AttributeModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/attributes")]
    [ApiController]
    public class AttributeController : ControllerBase
    {
        private readonly IAttributeService _attributeService;

        public AttributeController(IAttributeService attributeService)
        {
            _attributeService = attributeService;
        }


        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AttributeFilterModel attributeFilterModel)
        {
            try
            {
                var result = await _attributeService.GetAll(attributeFilterModel);
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
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AttributeAddModel attributeAddModel)
        {
            try
            {
                var result = await _attributeService.Add(attributeAddModel);
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
        [HttpGet("{id}/attribute-values")]
        public async Task<IActionResult> GetAttributeValueById(Guid id)
        {
            try
            {
                var result = await _attributeService.GetAttributeValueById(id);
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
