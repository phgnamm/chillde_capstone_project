using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class OrderTracking : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public OrderTrackingType Type { get; set; }

    // Foreign key
    public Guid OrderId { get; set; }

    // Relationship
    public Order Order { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;

    public virtual ICollection<OrderTrackingImage> OrderTrackingImages { get; set; } = new List<OrderTrackingImage>();
}