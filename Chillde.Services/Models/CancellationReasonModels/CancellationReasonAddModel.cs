using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.CancellationReasonModels
{
    public class CancellationReasonAddModel
    {
        [Required]
        [MinLength(10, ErrorMessage = "Name at least 10 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Value is required.")]
        [Range(typeof(float), "0.01", "79228162514264337593543950335", ErrorMessage = "Value must be greater than 0.")]
        public float Value { get; set; }


        [Required]
        [EnumDataType(typeof(Chillde.Repositories.Enums.Role), ErrorMessage = "Invalid RoleType.")]
        public Chillde.Repositories.Enums.Role RoleType { get; set; }
    }
}
