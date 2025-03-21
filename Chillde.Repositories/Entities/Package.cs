using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Package : BaseEntity
{
    public PackageName Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }

    #region new fields
    public int? DeliveryTime { get; set; }
    public int? SketchRevision { get; set; }
    public int? MaxQuantiy { get; set; }
    public float ResponseTime { get; set; }

    #endregion

    // Foreign key
    public Guid? ServiceId { get; set; }
    public Guid? OfferId { get; set; }

    // Relationship
    public Service? Service { get; set; }
    public Offer? Offer { get; set; }
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}