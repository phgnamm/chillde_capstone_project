using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.AttributeValueModels
{
    public class AttributeValueModel : BaseEntity
    {
        public string? Value { get; set; }
        public int IntOrder { get; set; }
        public Guid? AttributeId { get; set; }
    }
}
