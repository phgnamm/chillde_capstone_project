using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/categorys")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryServive _categoryService;

        public CategoryController(ICategoryServive categoryService)
        {
            _categoryService = categoryService;
        }
//      [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] CategoryAddModel categoryAddModel)
        {
            try
            {
                var result = await _categoryService.Add(categoryAddModel);
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
