using Chillde.Repositories.Enums;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.OfferAttachmentModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferUpdateModel
    {
        public OfferStatus? Status { get; set; }
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; }
        public Guid? ServiceId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "MinWeight must be greater than 0")]
        public float? MinWeight { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "MaxWeight must be greater than 0")]
        public float? MaxWeight { get; set; }
        public List<OfferAttachmentAddModel>? OfferAttachmentAddModels { get; set; } = new List<OfferAttachmentAddModel>();
        public List<Guid>? OfferAttachmentIdsDeleting { get; set; }
        public PackageUpdateModel? PackageUpdateModel { get; set; }
        public List<FeatureUpdateModel>? featureUpdateModels { get; set; }

    }
}
