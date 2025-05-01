using Chillde.API.Helpers;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
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
        //[Authorize]
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
        //[Authorize]
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
        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CategoryFilterModel categoryFilterModel)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var result = await _categoryService.GetAll(categoryFilterModel, sourceLanguageCode, targetLanguageCode);
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
        //[Authorize]
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

        //[Authorize]
        [HttpDelete("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteCategory(Guid id)
        {
            try
            {
                var response = await _categoryService.SoftDelete(id);
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
        [HttpGet("{idOrSlug}")]
        public async Task<IActionResult> GetById(string idOrSlug)
        {
            try
            {
                var acceptLanguage = Request.Headers["Accept-Language"].ToString();
                var sourceLanguageCode = LanguageHelper.GetSourceLanguageCode(acceptLanguage);
                var targetLanguageCode = LanguageHelper.GetTargetLanguageCode(sourceLanguageCode);
                var response = await _categoryService.GetByIdOrSlug(idOrSlug, sourceLanguageCode, targetLanguageCode);
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
