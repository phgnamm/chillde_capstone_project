using Chillde.Repositories.Enums;
using Chillde.Services.Models.ServiceAttachmentModels;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceUpdateModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public ServiceStatus Status { get; set; }
        public float? MinWeight { get; set; }
        public float? MaxWeight { get; set; }

        public List<AttachmentAddModel>? ServiceAttachments { get; set; }

        public List<Guid>? ServiceAttachmentIdsDeleting { get; set; }

    }
}
