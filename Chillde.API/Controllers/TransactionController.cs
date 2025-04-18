using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.WalletHistoryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chillde.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _walletHistoryService;

        public TransactionController(ITransactionService walletHistoryService)
        {
            _walletHistoryService = walletHistoryService;
        }

        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTransactionsFromUser([FromQuery] TransactionFilterModel transactionFilterModel)
        {
            try
            {
                var result = await _walletHistoryService.GetAllTransactionsFromUser(transactionFilterModel);
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
