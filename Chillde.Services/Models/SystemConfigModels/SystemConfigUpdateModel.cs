using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.SystemConfigModels
{
    public class SystemConfigUpdateModel
    {
        [Required] 
        public object Value { get; set; } = null!;
        public DateOnly? EffectiveFrom { get; set; }
    }
}