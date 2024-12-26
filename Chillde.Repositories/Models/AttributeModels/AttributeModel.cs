using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.AttributeModels
{
    public class AttributeModel : BaseEntity
    {
        public required string Name { get; set; }
        public bool IsRequired { get; set; } = true;
        public ItemAttributeType Type { get; set; }
    }
}
