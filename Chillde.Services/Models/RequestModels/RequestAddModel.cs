using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;


namespace Chillde.Services.Models.RequestModels
{
    public class RequestAddModel
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Decimal MinBudget { get; set; }
        public required Decimal MaxBudget { get; set; }
        public int Timeline { get; set; }
        public required Guid ItemId { get; set; }
        public required Guid AccountId { get; set; }
        public ICollection<AttachmentModel> Attachments { get; set; } = new List<AttachmentModel>();
        public ICollection<RequestDetailAddModel> RequestDetailAddModels { get; set; } = new List<RequestDetailAddModel>();

    }
    public class RequestDetailAddModel
    {
        public required string Description { get; set; }
        public required Guid AttributeId { get; set; }
    }

    public class AttachmentModel
    {
        public IFormFile? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
