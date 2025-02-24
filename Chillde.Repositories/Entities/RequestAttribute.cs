using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class RequestAttribute : BaseEntity
    {
       
        public required string Name { get; set; }
        public ItemAttributeType Type { get; set; }

        // Foreign key
        public Guid RequestId { get; set; }

        //// Relationship
        public Request Request { get; set; } = null!;
        public virtual ICollection<RequestAttributeValue>? RequestAttributeValues { get; set; } = new List<RequestAttributeValue>();
        public virtual ICollection<RequestAttributeAttachment>? RequestAttributeAttachments { get; set; } = new List<RequestAttributeAttachment>();
    }
}
