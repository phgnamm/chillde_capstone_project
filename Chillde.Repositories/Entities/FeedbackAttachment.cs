namespace Chillde.Repositories.Entities;

public class FeedbackAttachment : BaseEntity
{
    public string? ImageUrl { get; set; }
    public string? AttachmentAlt {  get; set; }
    public string? AttachmentUrl { get; set; }

    // Foreign key
    public Guid FeedbackId { get; set; }

    // Relationship
    public Feedback Feedback { get; set; } = null!;
}