using System.ComponentModel.DataAnnotations;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.MessageModels;

public class MessageDeleteModel
{
    [Required] public MessageDeleteType MessageDeleteType { get; set; }
}