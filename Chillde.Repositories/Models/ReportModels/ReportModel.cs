using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.ReportAttachmentModels;

namespace Chillde.Repositories.Models.ReportModels
{
    public class ReportModel : BaseEntity
    {
        public required string Description { get; set; }
        public string? Response { get; set; }
        public ReportStatus Status { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? OrderCreationDate { get; set; }
        public virtual List<ReportAttachmentModel>? ReportAttachments { get; set; }
        public string? CustomerName { get; set; }
    }
}
