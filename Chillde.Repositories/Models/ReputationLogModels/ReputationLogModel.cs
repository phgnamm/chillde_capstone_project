using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ReputationLogModels
{
    public class ReputationLogModel : BaseEntity
    {
        public string OrderCode { get; set; }
        public int PointChange { get; set; }
        public string Reason { get; set; } = null!;
    }
}
