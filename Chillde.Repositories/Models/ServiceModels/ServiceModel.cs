using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;

namespace Chillde.Repositories.Models.ServiceModels
{
    public class ServiceModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? ServiceImage { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double? Similarity { get; set; }
        public bool? IsOffer { get; set; }
        public ServiceStatus Status { get; set; }
        public List<ServiceAttachment>? ServiceAttachments { get; set; }
        public double? Rate { get; set; }
        public int? FeedbackCount { get; set; }
        public decimal? Price { get; set; }
        public AccountLiteModel? Artisan { get; set; }
    }
 
}
