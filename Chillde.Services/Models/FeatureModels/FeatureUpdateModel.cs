using Chillde.Repositories.Enums;
using Chillde.Services.Models.PackageFeatureModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.FeatureModels
{
    public class FeatureUpdateModel
    {
        public Guid? FeatureId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Question { get; set; }
        public MediaType QuestionType { get; set; }
        public bool IsInformationRequired { get; set; }
        public bool IsQuantity { get; set; }
        public int Index { get; set; }
        [Required]
        public required List<PackageFeatureAddModelForFeature> PackageFeatureAddModels { get; set; }
    }

    public class FeatureUpdateModelForFeatureService
    {
        [Required]
        public string Name { get; set; }
        public string? Question { get; set; }
        public bool IsInformationRequired { get; set; }
        public bool IsQuantity { get; set; }
        public int Index { get; set; }
    }
}
