using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class RequestAttributeAttachment : BaseEntity
    {
        public string? AttachmentAlt { get; set; }
        public string? AttachmentUrl { get; set; }

        // Foreign key
        public Guid RequestAttributeId { get; set; }

        // Relationship
        public RequestAttribute? RequestAttribute { get; set; }
    }
}
