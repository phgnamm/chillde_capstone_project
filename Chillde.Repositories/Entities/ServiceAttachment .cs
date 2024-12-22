namespace Chillde.Repositories.Entities;

public class ServiceAttachment : BaseEntity
{
    public string? ImageUrl { get; set; }

    // Foreign key
    public Guid ServiceId { get; set; }

    // Relationship
    public Service Service { get; set; } = null!;
}