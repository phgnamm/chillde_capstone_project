using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Deposit : BaseEntity
    {
        public decimal? Amount { get; set; }
        public DepositType Type { get; set; }

        public DepositStatus Status { get; set; } = DepositStatus.Pending;

        // Foreign key
        public Guid WalletId { get; set; }
        public Guid? OrderId { get; set; }

        // Relationship
        public Wallet Wallet { get; set; } = null!;
    }
}
