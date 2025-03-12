using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Feature : BaseEntity
{
    public string? Name { get; set; }

    #region new fields
    public string? Question { get; set; } 
    public MediaType QuestionType { get; set; } = MediaType.Text;
    public bool IsInformationRequired { get; set; }
    public bool IsQuantity { get; set; }
    #endregion

    // Foreign key
    public Guid? OfferId { get; set; }
    
    // Relationship
    public Offer? Offer { get; set; }
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
}