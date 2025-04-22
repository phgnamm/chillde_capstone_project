using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.AccountModels;
using Microsoft.AspNetCore.Http;


namespace Chillde.Repositories.Models.RequestModels
{
    public class RequestGetByIdModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public int Timeline { get; set; }
        public RequestStatus Status { get; set; } 
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemImageUrl { get; set; }
        public bool? IsCurrentAccountOffer { get; set; }
        public AccountLiteModel? AccountLiteModel { get; set; }
        public List<RequestAttributeGetModel> RequestAttributeGetModels { get; set; } = new List<RequestAttributeGetModel>();
        public List<RequestAttachmentGetModel>? RequestAttachmentGetModels { get; set; } = new List<RequestAttachmentGetModel>();
    }
    public class RequestAttributeGetModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public ItemAttributeType Type { get; set; }
        public List<RequestAttributeValueGetModel>? RequestAttributeValueGetModels { get; set; } = new List<RequestAttributeValueGetModel>();
        public List<RequestAttributeAttachmentGetModel>? RequestAttributeAttachmentGetModels { get; set; } = new List<RequestAttributeAttachmentGetModel>();

    }
    public class RequestAttributeValueGetModel
    {
        public Guid Id { get; set; }
        public string? Value { get; set; }
    }
    public class RequestAttributeAttachmentGetModel
    {
        public Guid Id { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
    public class RequestAttachmentGetModel
    {
        public Guid Id { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentAlt { get; set; }
    }
}
