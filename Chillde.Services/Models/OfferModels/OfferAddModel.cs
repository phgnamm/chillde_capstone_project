using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.OfferAttachmentModels;
using Chillde.Services.Models.PackageModels;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferAddModel
    {
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; } 
        public Guid? ServiceId { get; set; }
        public List<OfferAttachmentAddModel>? OfferAttachmentAddModels { get; set; } = new List<OfferAttachmentAddModel>();
        public List<FeatureAddModel>? FeatureAddModels { get; set; } = new List<FeatureAddModel>();
        public PackageAddModel? PackageAddModel { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "MinWeight must be greater than 0")]
        public float? MinWeight { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "MaxWeight must be greater than 0")]
        public float? MaxWeight { get; set; }

        [CustomValidation(typeof(OfferAddModel), nameof(ValidateWeightRange))]
        public static ValidationResult? ValidateWeightRange(OfferAddModel model, ValidationContext context)
        {
            if (model.MinWeight.HasValue && model.MaxWeight.HasValue && model.MinWeight.Value >= model.MaxWeight.Value)
            {
                return new ValidationResult("MaxWeight must be greater than MinWeight.");
            }
            return ValidationResult.Success;
        }

    }
}
