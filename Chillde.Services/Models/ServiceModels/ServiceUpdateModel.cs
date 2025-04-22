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

        public List<AttachmentAddModel>? AttachmentsToAdd { get; set; }

        public List<Guid>? AttachmentIdsToDelete { get; set; }

    }
}
