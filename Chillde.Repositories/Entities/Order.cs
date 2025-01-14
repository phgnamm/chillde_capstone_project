using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Order : BaseEntity
{
    public string? Phone { get; set; }
    public string Address { get; set; }
    public string Ward { get; set; }
    public int District { get; set; }
    public string Province { get; set; }
    public decimal? TotalPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime? OrderDateTime { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShipmentCode { get; set; }

    // Foreign key
    public Guid PaymentId { get; set; }
    public Guid PackageId { get; set; }
    public Guid ShipmentId { get; set; }


    // Relationship
    public Package Package { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;
    public Shipment Shipment { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
}