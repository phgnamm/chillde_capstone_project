using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.WalletHistoryModels
{
    public class TransactionFilterModel : FilterParameter
    {
        public Guid? AccountId { get; set; }
    }
}
