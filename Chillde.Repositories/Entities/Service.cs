using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Service : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Rate { get; set; }
    public int? FeedbackCount { get; set; }
    public ServiceStatus Status { get; set; }
    public float? MinWeight { get; set; }
    public float? MaxWeight { get; set; }

    // Foreign key
    public Guid CategoryId { get; set; }
    public List<string> Keywords { get; set; } = new List<string>();
    public float[] EmbeddingVector { get; set; } = Array.Empty<float>();

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public virtual ICollection<FAQ> FAQs { get; set; } = new List<FAQ>();
    public virtual ICollection<ServiceWishlist> ServiceWishlists { get; set; } = new List<ServiceWishlist>();
    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public virtual ICollection<ServiceAttachment > ServiceAttachments { get; set; } = new List<ServiceAttachment>();
    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
}