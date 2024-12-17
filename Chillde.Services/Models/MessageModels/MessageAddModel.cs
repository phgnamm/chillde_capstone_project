using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.MessageModels;

public class MessageAddModel
{
    // TODO: Upload files
    [Required] public string Content { get; set; } = null!;
}