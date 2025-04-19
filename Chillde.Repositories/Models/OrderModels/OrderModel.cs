using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.ServiceModels;

namespace Chillde.Repositories.Models.OrderModels
{
    public class OrderModel : BaseEntity
    {
        public string Name { get; set; } 
        public List<ServiceAttachment> Attachments { get; set; } = new List<ServiceAttachment>();
        public string ShipmentId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string ToWard { get; set; }
        public string ToDistrict { get; set; }
        public string ToProvince { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? PackagePrice { get; set; }
        public decimal? AdminCommUsedVch { get; set; }
        public decimal? AdminCommDefault { get; set; }
        public decimal? AdminComm { get; set; }
        public decimal? ArtistRevenue { get; set; }
        public required PackageName PackageName { get; set; }
        public int? Quantity { get; set; }
        public string? ShipmentCode { get; set; }
        public required OrderStatus Status { get; set; }
        public required OrderStage OrderStage { get; set; }
        public Guid? CreatedById { get; set; }
        public AccountLiteModel Artisan { get; set; }
        public ServiceModel ServiceModel { get; set; }
        public OfferModel OfferModel { get; set; }
    }
}
