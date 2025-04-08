using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.PackageModels;

namespace Chillde.Repositories.Models.OrderModels
{
    public class OrderDetailModel : BaseEntity
    {
        public string Code { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public int Quantity { get; set; }
        public string ShipmentCode { get; set; }
        public float? DeliveryTime { get; set; }
        public DateTime? StartTime { get; set; }
        public OrderStage Stage { get; set; }
        public OrderStatus Status { get; set; }
        public decimal OriginPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal? ArtistRevenueAfterCancel { get; set; } = null;
        public decimal? AfterApplyVoucherPrice { get; set; }
        public decimal? AdminCommUsedVch { get; set; }
        public decimal? AdminCommDefault { get; set; } 
        public decimal? ArtistRevenue { get; set; } 
        public decimal? VoucherCost { get; set; }
        public int CurrentSketchRevision { get; set; }
        public string? CancelOrderReason { get; set; }
        public string? AutoCancelOrderReason { get; set; }
        public AccountLiteModel? Customer { get; set; }
        public AccountLiteModel? Artisan { get; set; }
        public PackageModel? PackageModel { get; set; }
        public OfferModel? OfferModel { get; set; }
        public List<VoucherUsageModel> VoucherUsages { get; set; }
        public List<OrderInformationModel> OrderInformation { get; set; }
    }

    public class VoucherUsageModel : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public string VouhcherCode { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountOriginalValue { get; set; }
        public UsageStatus UsageStatus { get; set; }
    }

    public class OrderInformationModel : BaseEntity
    {
        public Guid FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public List<OrderAttachmentModel> Attachments { get; set; }
    }

    public class OrderAttachmentModel : BaseEntity
    {
        public string AttachmentUrl { get; set; }
        public string AttachmentAlt { get; set; }
    }

}
