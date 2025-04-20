using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.VoucherUsageLogModels
{
    public class VoucherUsageLogFilterModel : FilterParameter
    {
        public Guid? OrderId { get; set; }
        public Guid? AccountId { get; set; }
        public decimal? MinDiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public UsageStatus? UsageStatus { get; set; }
    }
}
