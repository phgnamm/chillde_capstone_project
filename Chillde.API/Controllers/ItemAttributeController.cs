using Chillde.Services.Interfaces;
using Chillde.Services.Models.ItemAttributeModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/item-attributes")]
    [ApiController]
    public class ItemAttributeController : ControllerBase
    {
        private readonly IItemAttributeService _itemAttributeService;

        public ItemAttributeController(IItemAttributeService itemAttributeService)
        {
            _itemAttributeService = itemAttributeService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ItemAttributeAddModel itemAttributeAddModel)
        {
            try
            {
                var result = await _itemAttributeService.Add(itemAttributeAddModel);
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
