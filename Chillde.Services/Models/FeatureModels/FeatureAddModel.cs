using Chillde.Repositories.Enums;
using Chillde.Services.Models.PackageFeatureModels;
using System.ComponentModel.DataAnnotations;
using Chillde.Services.Models.PackageModels;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureAddModel
    {
        [Required]
        public Guid PackageId { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Question { get; set; }
        [Required]
        public MediaType QuestionType { get; set; }
        [Required]
        public bool IsInformationRequired { get; set; }
        [Required]
        public bool IsQuantity { get; set; }
        [Required]
        public required List<PackageFeatureAddModelForFeature> PackageFeatureAddModels { get; set; }
    }
}
