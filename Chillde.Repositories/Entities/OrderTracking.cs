using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class OrderTracking : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsAccepted { get; set; } = null;

    #region new fields
    public OrderStage Stage { get; set; }
    public OrderTrackingType Type { get; set; }
    public int ExtendedDays { get; set; } 
    #endregion


    // Foreign key
    public Guid OrderId { get; set; }

    // Relationship
    public Order Order { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;

    public virtual ICollection<OrderTrackingAttachment>? OrderTrackingAttachments { get; set; } = new List<OrderTrackingAttachment>();
}