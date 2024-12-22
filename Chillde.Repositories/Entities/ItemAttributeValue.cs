using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class ItemAttributeValue : BaseEntity
    {
        public string? Value {  get; set; }
        public int IntOrder { get; set; }

        // Foreign key
        public Guid ItemAttributeId { get; set; }

        // Relationship
        public ItemAttribute ItemAttribute { get; set; } = null!;
    }
}
