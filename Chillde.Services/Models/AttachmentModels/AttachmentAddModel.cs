using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceAttachmentModels
{
    public class AttachmentAddModel
    {
        [Required] public string AttachmentAlt { get; set; } = null!;
        [Required] public string AttachmentUrl { get; set; } = null!;

        // [Required]
        // public IFormFile AttachmentUrl { get; set; }
    }
        
}
