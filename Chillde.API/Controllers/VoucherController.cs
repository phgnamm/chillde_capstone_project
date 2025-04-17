using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Chillde.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/vouchers")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }
        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> Add([FromBody] VoucherAddModel voucherAddModel)
        {
            try
            {
                var result = await _voucherService.Add(voucherAddModel);
                if (result.Status)
                {
                    return Ok(result);
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
        [HttpPut()]
        public async Task<IActionResult> Update([FromQuery] Guid id,[FromBody] VoucherUpdateModel voucherUpdateModel)
        {
            try
            {
                var result = await _voucherService.Update(id, voucherUpdateModel);
                if (result.Status)
                {
                    return Ok(result);
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
        [HttpPut("stop")]
        public async Task<IActionResult> StopVoucher([FromQuery] Guid id)
        {
            try
            {
                var result = await _voucherService.StopVoucher(id);
                if (result.Status)
                {
                    return Ok(result);
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
        [HttpGet()]
        public async Task<IActionResult> GetAll([FromQuery] VoucherFilterModel voucherFilterModel)
        {
            try
            {
                var result = await _voucherService.GetAll(voucherFilterModel);
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
        public async Task<IActionResult> GetAsync(Guid id)
        {
            try
            {
                var result = await _voucherService.Get(id);
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
