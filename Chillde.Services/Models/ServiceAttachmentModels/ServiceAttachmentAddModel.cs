using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceAttachmentModels
{
    public class ServiceAttachmentAddModel
    {
        [Required]
        public required string AttachmentAlt { get; set; }
        [Required]
        public required IFormFile AttachmentUrl { get; set; }
    }
}
