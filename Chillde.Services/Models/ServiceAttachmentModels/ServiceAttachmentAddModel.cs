using Chillde.Services.Models.CategoryModels;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceAttachmentModels
{
    public class ServiceAttachmentAddModel
    {
        [Required]
        public string AttachmentAlt { get; set; }
        [Required]
        public IFormFile AttachmentUrl { get; set; }
    }
        
}
