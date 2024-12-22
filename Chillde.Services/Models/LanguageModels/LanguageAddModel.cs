using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.LanguageModels
{
    public class LanguageAddModel
    {
        [Required(ErrorMessage = "Language code is required")]
        public LanguageCode Code { get; set; }

        [Required(ErrorMessage = "Language Name is required")]
        [StringLength(100, ErrorMessage = "Language names cannot exceed 100 characters")]
        public required string Name { get; set; }
    }
}
