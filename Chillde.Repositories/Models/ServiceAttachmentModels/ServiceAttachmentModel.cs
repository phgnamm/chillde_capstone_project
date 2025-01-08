using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ServiceAttachmentModels
{
    public class ServiceAttachmentModel : BaseEntity
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }
        public Guid ServiceId { get; set; }
    }
}
