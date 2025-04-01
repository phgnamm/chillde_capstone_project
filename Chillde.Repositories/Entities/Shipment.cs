using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Shipment : BaseEntity
{
    public string? TrackingId { get; set; }
    public ShipmentStatus CurrentStatusId { get; set; }
    public string? PartnerId { get; set; }
    public string? Label { get; set; }
    public string? Area { get; set; }
    public decimal Fee { get; set; }
    public decimal InsuranceFee { get; set; }
    public string? EstimatedPickTime { get; set; }
    public string? EstimatedDeliverTime { get; set; }
    // Foreign key
    public Guid OrderId { get; set; }

    // Relationship
    public Order Order { get; set; } = null!;
    public virtual ICollection<ProductShipment> ProductShipments { get; set; } = new List<ProductShipment>();
    public virtual ICollection<ShipmentStatusHistory> ShipmentStatusHistorys { get; set; } = new List<ShipmentStatusHistory>();
}