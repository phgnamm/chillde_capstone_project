using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.AttributeModels
{
    public class AttributeAddModel
    {
        public required string Name { get; set; }
        public required ItemAttributeType Type { get; set; }
        public ICollection<AttributeValueModel>? AttributeValueModels { get; set; } = new List<AttributeValueModel>();
    }
    public class AttributeValueModel
    {
        public string? Value { get; set; }
    }
}
