using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/features")]
    [ApiController]
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeatureController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        //[Authorize("Artist")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] FeatureUpdateModel featureUpdateModel, Guid id)
        {
            try
            {
                var result = await _featureService.UpdateAsync(featureUpdateModel, id);
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
