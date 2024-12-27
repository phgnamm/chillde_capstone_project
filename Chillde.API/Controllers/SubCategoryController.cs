using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ItemModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/subcategories")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryService _subcategoryService;

        public SubCategoryController(ISubCategoryService subcategoryService)
        {
            _subcategoryService = subcategoryService;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] SubCategoryUpdateModel subcategoryUpdateModel)
        {
            try
            {
                var result = await _subcategoryService.Update(id, subcategoryUpdateModel);
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
        [HttpPost("{subCategoryId}/items")]
        public async Task<IActionResult> AddSubcategory(Guid subCategoryId, [FromForm] ItemAddRangeModel itemAddRangeModel)
        {
            try
            {
                var result = await _subcategoryService.AddItem(subCategoryId, itemAddRangeModel);
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
        [HttpGet("{subCategoryId}/items")]
        public async Task<IActionResult> GetSubcategories(Guid subCategoryId, [FromQuery] ItemFilterModel itemFilterModel)
        {
            try
            {
                var response = await _subcategoryService.GetItemBySubCategory(subCategoryId, itemFilterModel);
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
