using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.WalletHistoryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/wallet-histories")]
    [ApiController]
    public class WalletHistoryController : ControllerBase
    {
        private readonly IWalletHistoryService _walletHistoryService;

        public WalletHistoryController(IWalletHistoryService walletHistoryService)
        {
            _walletHistoryService = walletHistoryService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllWalletHistoryFromUser([FromQuery] WalletHistoryFilterModel walletHistoryFilterModel)
        {
            try
            {
                var result = await _walletHistoryService.GetAllWalletHistoryFromUser(walletHistoryFilterModel);
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
