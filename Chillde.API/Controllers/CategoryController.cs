using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/categories")]
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
        //      [Authorize]
        [HttpPost("range")]
        public async Task<IActionResult> AddRange([FromForm] CategoryAddRangeModel categoryAddRangeModel)
        {
            try
            {                         
                var result = await _categoryService.AddList(categoryAddRangeModel);
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
        //        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CategoryFilterModel categoryFilterModel)
        {
            try
            {
                var result = await _categoryService.GetAll(categoryFilterModel);
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
        //      [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] CategoryUpdateModel categoryUpdateModel)
        {
            try
            {
                var result = await _categoryService.Update(id, categoryUpdateModel);
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
        //      [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var response = await _categoryService.Delete(id);
                return StatusCode(response.Code, response);

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
        //      [Authorize]
        [HttpPost("{categoryId}/sub-categories")]
        public async Task<IActionResult> AddSubcategory(Guid categoryId, [FromForm] SubCategoryAddRangeModel subCategoryAddRangeModel)
        {
            try
            {           
                var result = await _categoryService.AddSubcategory(categoryId, subCategoryAddRangeModel);
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
        //      [Authorize]
        [HttpGet("{categoryId}/sub-categories")]
        public async Task<IActionResult> GetSubcategories(Guid categoryId, [FromQuery] SubCategoryFilterModel subCategoryFilterModel)
        {
            try
            {
                var response = await _categoryService.GetSubcategoriesByCategory(categoryId, subCategoryFilterModel);
                return StatusCode(response.Code, response);
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
