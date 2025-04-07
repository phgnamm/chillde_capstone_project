using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageFeatureModels;

namespace Chillde.Repositories.Models.PackageModels
{
    public class PackageModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? DeliveryTime { get; set; }
        public int? SketchRevision { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int? MaxQuantity { get; set; }

        public Guid? ServiceId { get; set; }
        public Guid? OfferId { get; set; }
        public List<PackageFeatureModel>? PackageFeatures { get; set; }
        public List<FeatureModel>? Features { get; set; }
    }
}
