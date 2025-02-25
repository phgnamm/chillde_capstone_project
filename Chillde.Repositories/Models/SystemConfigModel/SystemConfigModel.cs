using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.SystemConfigModel
{
    public class SystemConfigModel : BaseEntity
    {
        public string FieldName { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public string? EntityType { get; set; }
    }
}
