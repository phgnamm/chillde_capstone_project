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

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Value must be greater than 0.")]
        public int Value { get; set; }

        [Required]
        [EnumDataType(typeof(Chillde.Repositories.Enums.Role), ErrorMessage = "Invalid RoleType.")]
        public Chillde.Repositories.Enums.Role RoleType { get; set; }
    }
}
