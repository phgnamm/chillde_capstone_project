namespace Chillde.Repositories.Entities;

public class FeedbackAttachment : BaseEntity
{
    public string? ImageUrl { get; set; }

    // Foreign key
    public Guid FeedbackId { get; set; }

    // Relationship
    public Feedback Feedback { get; set; } = null!;
}