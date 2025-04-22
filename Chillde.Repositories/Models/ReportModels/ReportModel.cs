using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.ReportAttachmentModels;

namespace Chillde.Repositories.Models.ReportModels
{
    public class ReportModel : BaseEntity
    {
        public required string Description { get; set; }
        public string? Response { get; set; }
        public string? OrderCode { get; set; }
        public string? ReportCode { get; set; }
        public string? CustomerImage { get; set; }
        public DateTime? CreationOrderDate { get; set; }
        public ReportStatus Status { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? OrderCreationDate { get; set; }
        public virtual List<ReportAttachmentModel>? ReportAttachments { get; set; }
        public string? CustomerName { get; set; }
    }

    public class ReportModelWithCountStatus
    {
        public int? Pending { get; set; }
        public int? Accepted { get; set; }
        public int? Rejected { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
