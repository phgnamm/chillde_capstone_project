using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherFilterModel : FilterParameter
    {
        public VoucherStatus? Status { get; set; }
        public decimal? MinDiscountValue { get; set; } = 0;
        public decimal? MaxDiscountValue { get; set; } = 0;
    }
}
