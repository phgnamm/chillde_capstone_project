using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceCollectionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/service-collections")]
    [ApiController]
    public class ServiceCollectionController : Controller
    {
        private readonly IServiceCollectionService _serviceCollectionService;

        public ServiceCollectionController(IServiceCollectionService serviceCollectionService)
        {
            _serviceCollectionService = serviceCollectionService;
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
                if (result.Code == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
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

        

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ServiceCollectionFilterModel filterModel)
        {
            try
            {
                var result = await _serviceCollectionService.GetAllAsync(filterModel);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = result.Message
                    });
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

        //[Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _serviceCollectionService.GetByIdAsync(id);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = result.Message
                    });
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

        // [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _serviceCollectionService.DeleteAsync(id);
                if (result.Code == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
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
