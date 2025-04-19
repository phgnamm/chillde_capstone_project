using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.WalletHistoryModels
{
    public class WalletHistoryModel : BaseEntity
    {
        public decimal? Amount { get; set; }
        public string? OrderCode { get; set; }
        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        public Guid WalletId { get; set; }
    }
}
