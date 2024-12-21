namespace Chillde.Repositories.Entities;

public class PackageFeature : BaseEntity
{
    public string? Question { get; set; }
    public bool? IsInformationRequired { get; set; }
    public bool? IsExtra { get; set; }
    public decimal? AdditionalCost { get; set; }
    public int? AdditionalDay { get; set; }

    // Foreign key
    public Guid PackageId { get; set; }
    public Guid FeatureId { get; set; }

    // Relationship
    public Package Package { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
}