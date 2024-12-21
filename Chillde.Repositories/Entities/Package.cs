namespace Chillde.Repositories.Entities;

public class Package : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }

    // Foreign key
    public Guid ServiceId { get; set; }

    // Relationship
    public Service Service { get; set; } = null!;
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}