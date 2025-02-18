using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceCollectionModels;
using Chillde.Services.Models.ServiceWishlistModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/service-collections")]
    [ApiController]
    public class ServiceCollectionController : Controller
    {
        private readonly IServiceCollectionService _serviceCollectionService;
        private readonly IServiceWishlistService _serviceWishlistService;

        public ServiceCollectionController(IServiceCollectionService serviceCollectionService, IServiceWishlistService serviceWishlistService)
        {
            _serviceCollectionService = serviceCollectionService;
            _serviceWishlistService = serviceWishlistService;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] ServiceCollectionAddModel serviceCollectionAddModel)
        {
            try
            {
                var result = await _serviceCollectionService.AddAsync(serviceCollectionAddModel);
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] ServiceCollectionAddModel serviceCollectionUpdateModel)
        {
            try
            {
                var result = await _serviceCollectionService.UpdateAsync(id, serviceCollectionUpdateModel);
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
        public async Task<IActionResult> GetAll([FromQuery] ServiceCollectionFilterModel filterModel)
        {
            try
            {
                var result = await _serviceCollectionService.GetAllAsync(filterModel);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _serviceCollectionService.GetByIdAsync(id);
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

        // [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _serviceCollectionService.DeleteAsync(id);
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

        [Authorize]
        [HttpPost("{serviceCollectionId}/service-wishlists")]
        public async Task<IActionResult> AddRange(Guid serviceCollectionId, [FromBody] List<Guid> serviceIds)
        {
            try
            {
                var result = await _serviceWishlistService.AddRangeAsync(serviceCollectionId, serviceIds);
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

        [Authorize]
        [HttpGet("{serviceCollectionId}/service-wishlists")]
        public async Task<IActionResult> GetAll(Guid serviceCollectionId, [FromQuery] ServiceWishlistFilterModel filterModel)
        {
            try
            {
                var result = await _serviceWishlistService.GetAllAsync(serviceCollectionId, filterModel);
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
