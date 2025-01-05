using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Order : BaseEntity
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal? TotalPrice { get; set; }
    public decimal? PackagePrice { get; set; }
    public int? Quantity { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Foreign key
    public Guid PackageId { get; set; }
    public Guid? ShipmentId { get; set; }


    // Relationship
    public Package Package { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;
    public Shipment? Shipment { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
}