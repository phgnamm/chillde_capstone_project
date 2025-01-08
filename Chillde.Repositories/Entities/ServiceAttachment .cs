using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities;

public class ServiceAttachment : BaseEntity
{
    public string? AttachmentAlt { get; set; }
    public string? AttachmentUrl { get; set; }

    // Foreign key
    public Guid ServiceId { get; set; }

    // Relationship
    [JsonIgnore]
    public Service Service { get; set; } = null!;
}