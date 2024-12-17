using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ConversationModels;

public class ConversationAddModel
{
    [Required] public Guid RecipientId { get; set; }
}