namespace Chillde.Repositories.Entities
{
    public class OrderInformationAttachment : BaseEntity
    {
        public string? AttachmentUrl {  get; set; }

        // Foreign key
        public Guid OrderInformationId { get; set; }

        // Relationship
        public OrderInformation OrderInformation { get; set; } = null!;
    }
}