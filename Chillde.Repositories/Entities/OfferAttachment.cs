namespace Chillde.Repositories.Entities;

public class OfferAttachment : BaseEntity
{
    public string? AttachmentAlt { get; set; }
    public string? AttachmentUrl { get; set; }
    
    // Foreign key
    public Guid OfferId { get; set; }
    
    // Relationship
    public Offer Offer { get; set; } = null!;  
}