using Chillde.Repositories.Enums;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceAddAllModel : ServiceAddModel
    {
        public List<PackageAddWithIdModel> PackageAddAllModels { get; set; } = new List<PackageAddWithIdModel>();
        public List<FeatureAddWithIdModel> FeatureAddModels { get; set; } = new List<FeatureAddWithIdModel>();
    }

    public class PackageAddWithIdModel
    {
        public Guid PackageId { get; set; }
        public PackageName Name { get; set; }
        [Required(ErrorMessage = "Package's description is required.")]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? DeliveryTime { get; set; }
        public int? SketchRevision { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        [Required]
        public required List<PackageFeatureAddWithPackageIdModel> PackageFeatureAddModels { get; set; }
    }

    public class FeatureAddWithIdModel
    {
        [Required]
        public Guid FeatureId { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Question { get; set; }
        public MediaType QuestionType { get; set; }
        public bool IsInformationRequired { get; set; }
        public bool IsQuantity { get; set; }
        public int Index { get; set; }
    }

    public class PackageFeatureAddWithPackageIdModel
    {
        public Guid? FeatureId { get; set; }
        // [Required]
        public string? Name { get; set; }
        public bool IsExtra { get; set; }
        public decimal? AdditionalCost { get; set; }
        public int? AdditionalDay { get; set; }
        public bool? IsChecked { get; set; }
        public int MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public int Index { get; set; }
    }
}
