using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Feature : BaseEntity
{
    public string? Name { get; set; }
    public string? Question { get; set; } 
    public MediaType QuestionType { get; set; } = MediaType.Text;
    public bool IsInformationRequired { get; set; }
    public bool IsQuantity { get; set; }
    public int Index { get; set; }


    // Relationship
    public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
}