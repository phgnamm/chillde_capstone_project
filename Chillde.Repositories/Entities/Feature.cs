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

    // Relationship
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
}