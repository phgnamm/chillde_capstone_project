using Chillde.Services.Models.CategoryModels;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceAttachmentModels
{
    public class ServiceAttachmentAddModel
    {
        [Required]
        public required List<string> AttachmentAlt { get; set; }
        [Required]
        public List<IFormFile>? AttachmentUrls { get; set; }
    }
    public class ServiceAttachmentAddRequestModel
    {
        public required string AttachmentAlt { get; set; }
    }
}
