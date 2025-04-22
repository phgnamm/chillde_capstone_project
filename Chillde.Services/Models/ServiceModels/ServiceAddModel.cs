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
        public float? MinWeight { get; set; }
        public float? MaxWeight { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        public List<AttachmentAddModel>? Attachments { get; set; }
    }
}
