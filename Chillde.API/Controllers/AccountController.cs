using Chillde.Services.Interfaces;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.DashBoardModels;
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
    [Authorize(Roles = "Customer")]
    [HttpGet("vouchers")]
    public async Task<IActionResult> GetVoucher([FromQuery] Guid packageId, [FromQuery] decimal totalPriceOfOrder)
    {
        try
        {
            var result = await _accountService.GetVoucher(packageId, totalPriceOfOrder);
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
    [HttpGet("admin-vouchers")]
    public async Task<IActionResult> GetVoucherAdmin(Guid orderId)
    {
        try
        {
            var result = await _accountService.GetVoucherAdmin(orderId);
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
    [HttpGet("search-histories")]
    public async Task<IActionResult> GetSearchHistories()
    {
        try
        {
            var result = await _accountService.GetSearchHistories();
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
    public async Task<IActionResult> Update(Guid id, [FromBody] AccountUpdateModel accountUpdateModel)
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
    public async Task<IActionResult> BecomeASeller(Guid id, [FromBody] AccountBecomeASellerModel accountBecomeASellerModel)
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
/*    [Authorize(Roles = "Admin")]*/
    [HttpGet("{artisanId}/categories")]
    public async Task<IActionResult> GetCategoryByService(Guid artisanId, [FromQuery] FilterModel filterModel)
    {
        try
        {
            var result = await _accountService.GetCategoryByArtisan(artisanId, filterModel);
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
    
    [HttpGet("{id}/shipping-addresses/default")]
    public async Task<IActionResult> GetDefaultShippingAddress(Guid id)
    {
        try
        {
            var result = await _accountService.GetDefaultShippingAddress(id);
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
    [HttpDelete("{id}/roles/{accountRoleId}")]
    public async Task<IActionResult> DeleteAccountRole(Guid id, Guid accountRoleId)
    {
        try
        {
            var result = await _accountService.DeleteAccountRole(id, accountRoleId);;
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
    [HttpPut("{id}/roles/{accountRoleId}/toggle")]
    public async Task<IActionResult> ToggleAccountRoleStatus(Guid id, Guid accountRoleId)
    {
        try
        {
            var result = await _accountService.ToggleAccountRoleStatus(id, accountRoleId);;
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

    [Authorize(Roles ="Artisan")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashBoard([FromQuery] DashboardFilterModel dashboardFilterModel)
    {
        try
        {
            var result = await _accountService.GetArtisanDashboard(dashboardFilterModel);
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

    [HttpGet("not-pagination")]
    public async Task<IActionResult> GetAllAccount([FromQuery] AccountFilterModel accountFilterModel)
    {
        try
        {
            var result = await _accountService.GetAllAccount(accountFilterModel);
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
    [HttpGet("dashboard-admin")]
    public async Task<IActionResult> GetAdminDashBoard([FromQuery] DashboardFilterModel dashboardFilterModel)
    {
        try
        {
            var result = await _accountService.GetAdminDashboard(dashboardFilterModel);
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

    //[Authorize(Roles = "Admin")]
    [HttpGet("dashboard-monthly-revenue-admin")]
    public async Task<IActionResult> GetRevenueByMonthOrCategory([FromQuery] DashboardFilterModel dashboardFilterModel)
    {
        try
        {
            var result = await _accountService.GetRevenueByMonth(dashboardFilterModel);
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

    //[Authorize(Roles = "Admin")]
    [HttpGet("dashboard-category-revenue-admin")]
    public async Task<IActionResult> GetRevenueByCategory([FromQuery] DashboardFilterModel dashboardFilterModel)
    {
        try
        {
            var result = await _accountService.GetRevenueByCategory(dashboardFilterModel);
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
    /*[HttpPost("ban-role")]
    public async Task<IActionResult> BanAccountRole([FromBody] BanAccountRoleModel request)
    {
        try
        {
            var result = await _accountService.BanAccountRole(request);
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
    }*/
}