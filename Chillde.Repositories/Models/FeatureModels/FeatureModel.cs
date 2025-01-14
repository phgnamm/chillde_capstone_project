using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.PackageFeatureModels;

namespace Chillde.Repositories.Models.FeatureModels
{
    public class FeatureModel : BaseEntity
    {
        public string? Name { get; set; }
        public List<PackageFeatureModel>? PackageFeatures { get; set; }
    }
}
