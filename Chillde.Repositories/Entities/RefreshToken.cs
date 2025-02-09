namespace Chillde.Repositories.Entities;

public class RefreshToken : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid Token { get; set; }
    public DateTime Expires { get; set; }

    // Foreign key
    public Guid AccountId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
}