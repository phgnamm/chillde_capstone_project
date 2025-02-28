using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.RequestModels
{
    public class RequestUpdateModel
    {
        public decimal? MinBudget { get; set; }

        public decimal? MaxBudget { get; set; }

        public int? Timeline { get; set; }

        public ICollection<RequestAttributeUpdateModel> RequestAttributes { get; set; } = new List<RequestAttributeUpdateModel>();

        public ICollection<RequestAttachmentUpdateModel> RequestAttachments { get; set; } = new List<RequestAttachmentUpdateModel>();
    }

    public class RequestAttributeUpdateModel
    {
        public Guid? Id { get; set; } 

        public string? Name { get; set; } 

        public ItemAttributeType? Type { get; set; }

        public ICollection<RequestAttributeValueUpdateModel>? RequestAttributeValueAddModels { get; set; } = new List<RequestAttributeValueUpdateModel>();

        public ICollection<RequestAttributeAttachmentUpdateModel>? RequestAttributeAttachmentAddModels { get; set; } = new List<RequestAttributeAttachmentUpdateModel>();
    }

    public class RequestAttributeValueUpdateModel
    {
        public Guid? Id { get; set; } 

        public string? Value { get; set; }
    }

    public class RequestAttributeAttachmentUpdateModel
    {
        public Guid? Id { get; set; } 

        public IFormFile? AttachmentUrl { get; set; }

        [MaxLength(500)]
        public string? AttachmentAlt { get; set; }
    }

    public class RequestAttachmentUpdateModel
    {
        public Guid? Id { get; set; }

        public IFormFile? AttachmentUrl { get; set; }

        [MaxLength(500)]
        public string? AttachmentAlt { get; set; }
    }


}
