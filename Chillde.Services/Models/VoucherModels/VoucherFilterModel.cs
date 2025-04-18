using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherFilterModel : FilterParameter
    {
        public Guid? ArtisanId { get; set; }
        public VoucherStatus? Status { get; set; }
        public decimal? MinDiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public VoucherType? VoucherType { get; set; }
    }
}
