using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.TranslationModels
{
    public class TransaltionAddModel
    {
        [Required(ErrorMessage = "TranslationText is required.")]
        public string TranslationText { get; set; } = null!;

        [Required(ErrorMessage = "Entity type is required.")]
        [StringLength(100, ErrorMessage = "Entity type cannot be longer than 100 characters.")]
        public string EntityType { get; set; } = null!;

        [Required(ErrorMessage = "Entity ID is required.")]
        public Guid EntityId { get; set; }

        [Required(ErrorMessage = "Field name is required.")]
        [StringLength(100, ErrorMessage = "Field name cannot be longer than 100 characters.")]
        public string FieldName { get; set; } = null!;
    }
}
