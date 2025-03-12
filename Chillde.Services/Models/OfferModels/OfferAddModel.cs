using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.OfferAttachmentModels;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferAddModel
    {
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; } 
        public Guid? ServiceId { get; set; }
        public List<OfferAttachmentAddModel>? OfferAttachmentAddModels { get; set; } = new List<OfferAttachmentAddModel>();
        public List<FeatureAddModel>? FeatureAddModels { get; set; } = new List<FeatureAddModel>();
        
    }
}
