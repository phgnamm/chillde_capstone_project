using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities
{
    public class ReportAttachment : BaseEntity
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }

        // Foreign key
        public Guid ReportId { get; set; }

        // Relationship
        [JsonIgnore]
        public Report Report { get; set; } = null!;

    }
}
