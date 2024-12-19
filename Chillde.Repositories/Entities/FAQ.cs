using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class FAQ : BaseEntity
    {
        public string? Question {  get; set; }
        public String? Answer { get; set; }

        // Foreign key
        public Guid ServiceId { get; set; }

        // Relationship
        public Service Service { get; set; } = null!;
    }
}
