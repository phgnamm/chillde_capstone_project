using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers;

[Route("api/v1/accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] AccountAddRangeModel accountAddRangeModel)
    {
        try
        {
            var result = await _accountService.AddRange(accountAddRangeModel);
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

    [HttpGet("{idOrUsername}")]
    public async Task<IActionResult> Get(string idOrUsername)
    {
        try
        {
            var result = await _accountService.Get(idOrUsername);
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AccountFilterModel accountFilterModel)
    {
        try
        {
            var result = await _accountService.GetAll(accountFilterModel);
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
    [HttpGet("vouchers")]
    public async Task<IActionResult> GetVoucher([FromQuery] Guid packageId)
    {
        try
        {
            var result = await _accountService.GetVoucher(packageId);
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
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] AccountUpdateModel accountUpdateModel)
    {
        try
        {
            var result = await _accountService.Update(id, accountUpdateModel);
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

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, [FromBody] AccountUpdateRolesModel accountUpdateRolesModel)
    {
        try
        {
            var result = await _accountService.UpdateRoles(id, accountUpdateRolesModel);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _accountService.Delete(id);
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

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        try
        {
            var result = await _accountService.Restore(id);
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
    
    [Authorize(Roles = "Customer")]
    [HttpPut("{id}/become-a-seller")]
    public async Task<IActionResult> BecomeASeller(Guid id, [FromForm] AccountBecomeASellerModel accountBecomeASellerModel)
    {
        try
        {
            var result = await _accountService.BecomeASeller(id, accountBecomeASellerModel);
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