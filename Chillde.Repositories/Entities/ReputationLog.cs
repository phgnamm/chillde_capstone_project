using Chillde.Repositories.Enums;
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

        //Foreign key
        public Guid AccountRoleId { get; set; }
        // Relationship
        public virtual AccountRole AccountRole { get; set; } = null!;
    }
}
