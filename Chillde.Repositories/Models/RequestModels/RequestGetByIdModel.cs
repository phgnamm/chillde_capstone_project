using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;


namespace Chillde.Repositories.Models.RequestModels
{
    public class RequestGetByIdModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public int Timeline { get; set; }
        
        public RequestStatus Status { get; set; } 
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemImageUrl { get; set; }
        public ICollection<RequestDetailGetByIdModel> RequestDetailGetByIdModels { get; set; } = new List<RequestDetailGetByIdModel>();
        public List<AttachmentGetModel>? Attachments { get; set; }
    }
    public class RequestDetailGetByIdModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public Guid ItemAttributeId { get; set; }
        public string ItemAttributeName { get; set; }
    }

    public class AttachmentGetModel
    {
        public string? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
