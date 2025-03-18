using System.ComponentModel.DataAnnotations;

namespace Chillde.Repositories.Models.SystemConfigModel;

public class SystemConfigUpdateModel
{
    [Required]
    public object Value { get; set; } = null!;
}