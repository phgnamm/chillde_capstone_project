using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.VoucherUsageModels
{
    public class VoucherUsageLogModel : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public Guid OrderId { get; set; }
        public decimal DiscountValue { get; set; } // giá giảm quy ra tiền 
        public decimal DiscountValueOrigin { get; set; } // phần trăm giảm ban đầu
        public UsageStatus UsageStatus { get; set; }
    }
}
