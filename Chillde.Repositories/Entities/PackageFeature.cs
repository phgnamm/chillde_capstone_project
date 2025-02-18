using Chillde.Repositories.Enums;
using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities;

public class PackageFeature : BaseEntity
{
    public string? Question { get; set; }
    public bool? IsInformationRequired { get; set; }
    public bool? IsExtra { get; set; }
    public decimal? AdditionalCost { get; set; }
    public int? AdditionalDay { get; set; }
    public bool? IsQuantity { get; set; }
    public MediaType? QuestionType { get; set; }

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