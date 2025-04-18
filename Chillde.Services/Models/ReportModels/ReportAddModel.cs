using Chillde.Services.Models.ServiceAttachmentModels;

namespace Chillde.Services.Models.ReportModels
{
    public class ReportAddModel
    {
        public required string Description { get; set; }
        public List<AttachmentAddModel>? Attachments { get; set; }
    }
}
