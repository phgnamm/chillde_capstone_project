namespace Chillde.Repositories.Entities;

public class Feature : BaseEntity
{
    public string? Name { get; set; }

    // Relationship
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
}