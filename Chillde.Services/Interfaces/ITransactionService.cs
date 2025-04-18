using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.WalletHistoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<ResponseModel> GetAllTransactionsFromUser(TransactionFilterModel transactionFilterModel);
    }
}
