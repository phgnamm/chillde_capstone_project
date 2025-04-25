using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.FeatureModels;

namespace Chillde.Repositories.Models.PackageFeatureModels
{
    public class PackageFeatureModel : BaseEntity
    {
        public string? Name { get; set; }
        public decimal? AdditionalCost { get; set; }
        public float? AdditionalDay { get; set; }
        public bool? IsExtra { get; set; }
        public bool? IsChecked { get; set; }
        public int MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public int Index { get; set; }
        public Guid? PackageId { get; set; }
        public Guid FeatureId { get; set; }
    }
}
