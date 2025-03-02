using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class ReputationLog : BaseEntity
    {
        public int PointChange { get; set; } // Số điểm thay đổi (+ hoặc -)
        public string Reason { get; set; } = null!; // Lý do thay đổi

        public Guid AccountId { get; set; }
        // Relationship
        public virtual Account Account { get; set; } = null!;
    }
}
