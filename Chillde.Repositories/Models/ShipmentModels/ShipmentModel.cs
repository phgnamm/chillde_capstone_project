
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentModel : BaseEntity
    {
        public string? TrackingId { get; set; }
        public ShipmentStatus StatusId { get; set; }
        public string? PartnerId { get; set; }
        public string? Label { get; set; }
        public string? Area { get; set; }
        public decimal Fee { get; set; }
        public decimal InsuranceFee { get; set; }
        public string? EstimatedPickTime { get; set; }
        public string? EstimatedDeliverTime { get; set; }
        // Foreign key
        public Guid OrderId { get; set; }
    }
}
