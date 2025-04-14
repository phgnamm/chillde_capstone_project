using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ReputationLogModels
{
    public class ReputationLogModel : BaseEntity
    {
        public int PointChange { get; set; }
        public string Reason { get; set; } = null!;
        public Guid OrderId { get; set; }
        public Guid AccountRoleId { get; set; }
    }
}
