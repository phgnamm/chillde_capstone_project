
namespace Chillde.Repositories.Entities
{
    public class RequestAttachment : BaseEntity
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }

        // Foreign key
        public Guid RequestId { get; set; }

        // Relationship
        public Request Request { get; set; } = null!;
    }
}
