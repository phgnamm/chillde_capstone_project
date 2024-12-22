namespace Chillde.Repositories.Entities;

public class Feedback : BaseEntity
{
    public int? Rating { get; set; }
    public string? Description { get; set; }

    // Foreign key
    public Guid ServiceId { get; set; }

    // Relationship
    public Service Service { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;
    public virtual ICollection<FeedbackAttachment> FeedbackAttachments { get; set; } = new List<FeedbackAttachment>();
}