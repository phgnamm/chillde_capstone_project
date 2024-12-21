namespace Chillde.Repositories.Entities;

public class OrderTrackingAttachment : BaseEntity
{
    public string? ImageUrl { get; set; }

    // Foreign key
    public Guid OrderTrackingId { get; set; }

    // Relationship
    public OrderTracking OrderTracking { get; set; } = null!;
}