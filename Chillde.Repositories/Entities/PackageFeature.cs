using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities;

public class PackageFeature : BaseEntity
{
    public string? Name { get; set; }

    #region new fields
    public decimal? AdditionalCost { get; set; }
    public int? AdditionalDay { get; set; }
    public bool? IsExtra { get; set; }
    public bool? IsChecked { get; set; }
    public int MaxQuantity { get; set; }
    #endregion

    // Foreign key
    public Guid PackageId { get; set; }
    public Guid FeatureId { get; set; }

    // Relationship
    [JsonIgnore]
    public Package Package { get; set; } = null!;
    [JsonIgnore]
    public Feature Feature { get; set; } = null!;
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
}