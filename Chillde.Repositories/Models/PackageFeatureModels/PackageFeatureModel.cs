using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.PackageFeatureModels
{
    public class PackageFeatureModel : BaseEntity
    {
        public string? Question { get; set; }
        public bool? IsInformationRequired { get; set; }
        public bool? IsExtra { get; set; }
        public decimal? AdditionalCost { get; set; }
        public int? AdditionalDay { get; set; }
    }
}
