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
        public bool IsOffer { get; set; }
        public ServiceStatus Status { get; set; }
        //public List<ServiceAttachmentAddModel>? ServiceAttachments { get; set; }
        
    }
}
