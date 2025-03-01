using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;


namespace Chillde.Services.Models.RequestModels
{
    public class RequestAddModel
    {
        [Required, MaxLength(255)]
        public required string Name { get; set; }

        [Required, MaxLength(1000)]
        public required string Description { get; set; }

        [Required, Range(0, double.MaxValue)]
        public required decimal MinBudget { get; set; }

        [Required, Range(0, double.MaxValue)]
        public required decimal MaxBudget { get; set; }

        [Range(0, int.MaxValue)]
        public int Timeline { get; set; }

        [Required]
        public required Guid ItemId { get; set; }

        [Required]
        public ICollection<RequestAttributeAddModel> RequestAttributeAddModels { get; set; } = new List<RequestAttributeAddModel>();
        public ICollection<RequestAttachmentAddModel>? RequestAttachmentAddModels { get; set; } = new List<RequestAttachmentAddModel>();
    }

    public class RequestAttributeAddModel
    {
        [Required, MaxLength(255)]
        public required string Name { get; set; }

        [Required]
        public required ItemAttributeType Type { get; set; }

        public ICollection<RequestAttributeAttachmentAddModel>? RequestAttributeAttachmentAddModels { get; set; } = new List<RequestAttributeAttachmentAddModel>();

        public ICollection<RequestAttributeValueAddModel>? RequestAttributeValueAddModels { get; set; } = new List<RequestAttributeValueAddModel>();
    }

    public class RequestAttributeAttachmentAddModel
    {
        public IFormFile? AttachmentUrl { get; set; }

        [MaxLength(500)]
        public string? AttachmentAlt { get; set; }
    }

    public class RequestAttributeValueAddModel
    {
        public string? Value { get; set; }
    }
    public class RequestAttachmentAddModel
    {
        public IFormFile? AttachmentUrl { get; set; }

        [MaxLength(500)]
        public string? AttachmentAlt { get; set; }
    }

}
