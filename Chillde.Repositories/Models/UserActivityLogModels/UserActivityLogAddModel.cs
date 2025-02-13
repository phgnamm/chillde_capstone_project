using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.UserActivityLogModels
{
    public class UserActivityLogAddModel
    {
        public Guid UserId { get; set; }
        public required string ActivityType { get; set; }
        public required string ActivityDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
