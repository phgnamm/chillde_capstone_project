using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Offer : BaseEntity
{
    public string? Message { get; set; }
    public OfferStatus Status { get; set; }
    public float? MinWeight { get; set; }
    public float? MaxWeight { get; set; }

    // Foreign key
    public Guid RequestId { get; set; }
    public Guid? ServiceId { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public Request Request { get; set; } = null!;
    public Service? Service { get; set; }
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<OfferAttachment> OfferAttachments { get; set; } = new List<OfferAttachment>();
}