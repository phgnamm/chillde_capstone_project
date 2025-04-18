using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherFilterModel : FilterParameter
    {
        public Guid? ArtisanId { get; set; }
        public VoucherStatus? Status { get; set; }
        public decimal? MinDiscountValue { get; set; } = 0;
        public decimal? MaxDiscountValue { get; set; } = 100;
        //public VoucherType VoucherType { get; set; }
    }
}
