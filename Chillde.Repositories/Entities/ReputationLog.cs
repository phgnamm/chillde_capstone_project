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
        public float PointChange { get; set; }
        public string Reason { get; set; } = null!;

        // Foreign keys
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public Guid AccountRoleId { get; set; }
        public virtual AccountRole AccountRole { get; set; } = null!;
    }
}
