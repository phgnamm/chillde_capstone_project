using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.SystemConfigModels
{
    public class SystemConfigUpdateModel
    {
        [Required] 
        public object Value { get; set; } = null!;
        [Required]
        public DateTime? EffectiveFrom { get; set; }
    }
}