using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public MediaType MessageType { get; set; } = MediaType.Text;
    public bool IsPinned { get; set; } = false;

    // Foreign key
    public Guid? ParentMessageId { get; set; }
    public Guid? OfferId { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public Message? ParentMessage { get; set; }
    public Offer Offer { get; set; } = null!;
    public virtual ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
}