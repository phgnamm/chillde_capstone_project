using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class Attribute : BaseEntity
    {
       
        public required string Name { get; set; }
        public bool IsRequired { get; set; } = true;
        public ItemAttributeType Type { get; set; }

        // Foreign key
        //public Guid ItemId { get; set; }

        //// Relationship
        //public Item Item { get; set; } = null!;
        //public RequestDetail RequestDetail { get; set; } = null!;
        public virtual ICollection<AttributeValue>? AttributeValues { get; set; } = new List<AttributeValue>();
        public virtual ICollection<ItemAttribute> ItemAttributes { get; set; } = new List<ItemAttribute>();
    }
}
