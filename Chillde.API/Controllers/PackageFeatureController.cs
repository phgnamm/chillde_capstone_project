using Chillde.Services.Interfaces;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/package-features")]
    [ApiController]
    public class PackageFeatureController : ControllerBase
    {
        private readonly IPackageFeatureService _packageFeatureService;

        public PackageFeatureController(IPackageFeatureService packageFeatureService)
        {
            _packageFeatureService = packageFeatureService;
        }

        //[Authorize("Artist")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] PackageFeatureUpdateModel model, Guid id)
        {
            try
            {
                var result = await _packageFeatureService.UpdateAsync(model, id);
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

        //[Authorize("Artist, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackageFeature(Guid id)
        {
            try
            {
                var result = await _packageFeatureService.DeletePackageFeatureAsync(id);
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
