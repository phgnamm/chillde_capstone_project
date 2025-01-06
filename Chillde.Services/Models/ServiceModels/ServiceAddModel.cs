using Chillde.Repositories.Entities;
using Chillde.Services.Models.ServiceAttachmentModels;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsOffter { get; set; }
        [Required]
        public Guid ItemId { get; set; }
        public List<ServiceAttachmentAddModel>? ServiceAttachments { get; set; }
    }
}
