using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferFilterModel : FilterParameter
    {
        public Guid? RequestId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? CreatedById { get; set; }
        public OfferStatus? Status { get; set; }
        public int? MinDeliveryTime { get; set; }
        public int? MaxDeliveryTime { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
