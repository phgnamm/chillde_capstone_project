namespace Chillde.Repositories.Models.ReportAttachmentModels
{
    public class ReportAttachmentModel
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }
        public Guid ReportId { get; set; }
    }
}
