using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherFilterModel : FilterParameter
    {
        public Guid? ArtisanId { get; set; }
        public VoucherStatus? Status { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal? MinDiscountValue { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? ExpiredTime { get; set; }
        public int? MinReputation {  get; set; }
        public int? MinOrderRequired { get; set; }
        public VoucherType? VoucherType { get; set; }
    }
}
