using Chillde.Repositories.Enums;
using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities;

public class Package : BaseEntity
{
    public PackageName Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? DeliveryTime { get; set; }
    public int? SketchRevision { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public float ResponseTime { get; set; }

    // Foreign key
    [JsonIgnore]
    public Guid? ServiceId { get; set; }
    [JsonIgnore]
    public Guid? OfferId { get; set; }

    // Relationship
    public Service? Service { get; set; }
    public Offer? Offer { get; set; }
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}