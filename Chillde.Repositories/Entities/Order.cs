using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Order : BaseEntity
{
    public required string Code { get; set; }
    public string? Phone { get; set; }
    public string Address { get; set; }
    public string ToWard { get; set; }
    public int ToDistrict { get; set; }
    public string ToProvince { get; set; }
    public decimal? TotalPrice { get; set; }
    public decimal? PackagePrice { get; set; }
    public int? Quantity { get; set; }
    public string? ShipmentCode { get; set; }

    #region new fields
    public DateTime? DeliveryTime { get; set; }
    public int? CurrentSketchRevision { get; set; }
    public OrderStage Stage { get; set; } = OrderStage.ReviewRequirement;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    #endregion

    // Foreign key
    public Guid PackageId { get; set; }

    // Relationship
    public Package Package { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
}