using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.ServiceAttachmentModels;

namespace Chillde.Repositories.Models.ServiceModels
{
    public class ServiceModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsOffter { get; set; }
        public ServiceStatus Status { get; set; }
        public List<ServiceAttachment>? ServiceAttachments { get; set; }
    }
}
