using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.SystemConfigModels
{
    public class SystemConfigAddModel
    {
        [Required]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        public object Value { get; set; } = null!;

        [Required]
        public ConfigType EntityType { get; set; }
    }
}
