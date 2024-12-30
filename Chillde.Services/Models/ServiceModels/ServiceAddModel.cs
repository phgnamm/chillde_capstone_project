using Chillde.Repositories.Entities;
using Chillde.Services.Models.ServiceAttachmentModels;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceAddModel
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required bool IsOffter { get; set; }
        [Required]
        public required Guid ItemId { get; set; }
        public List<ServiceAttachmentAddModel>? ServiceAttachments { get; set; }
    }
}
