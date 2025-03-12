using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.OfferAttachmentModels;

public class OfferAttachmentAddModel
{
    [Required]
    public string? AttachmentAlt { get; set; }
    [Required]
    public IFormFile? AttachmentUrl { get; set; }
}