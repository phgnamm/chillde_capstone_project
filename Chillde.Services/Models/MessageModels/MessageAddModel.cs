using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.MessageModels;

public class MessageAddModel
{
    // TODO: Upload files
    public string Content { get; set; } = null!;
    public IFormFile? Attachment { get; set; }
}