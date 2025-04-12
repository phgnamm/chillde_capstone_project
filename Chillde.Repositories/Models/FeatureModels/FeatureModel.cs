using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.PackageFeatureModels;

namespace Chillde.Repositories.Models.FeatureModels
{
    public class FeatureModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Question { get; set; }
        public MediaType QuestionType { get; set; }
        public bool IsInformationRequired { get; set; }
        public bool IsQuantity { get; set; }
        public int Index { get; set; }
        public List<PackageFeature>? PackageFeatures { get; set; }
    }
}
