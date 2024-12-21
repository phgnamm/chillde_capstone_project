namespace Chillde.Repositories.Entities;

public class FAQ : BaseEntity
{
    public string? Question { get; set; }
    public string? Answer { get; set; }

    // Foreign key
    public Guid ServiceId { get; set; }

    // Relationship
    public Service Service { get; set; } = null!;
}