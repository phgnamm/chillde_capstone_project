namespace Chillde.Repositories.Entities;

public class RefreshToken : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Guid Token { get; set; }
    public DateTime Expires { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
}