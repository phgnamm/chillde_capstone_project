using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class UserActivityLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public  required string ActivityType { get; set; } // e.g., Search, Click, View
        public required string ActivityDetails { get; set; } // Thông tin chi tiết về hoạt động
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public float[] EmbeddingVector { get; set; } = Array.Empty<float>();
    }
}
