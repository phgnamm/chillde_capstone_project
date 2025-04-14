using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
namespace Chillde.Repositories.Models.DepositModels
{
    public class DepositModel : BaseEntity
    {
        public decimal? Amount { get; set; }
        public DepositType Type { get; set; }
        public DepositStatus Status { get; set; }
        public Guid WalletId { get; set; }
        public Guid OrderId { get; set; }
    }
}
