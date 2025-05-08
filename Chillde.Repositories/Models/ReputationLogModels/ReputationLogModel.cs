using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ReputationLogModels
{
    public class ReputationLogModel : BaseEntity
    {
        public string OrderCode { get; set; }
        public float PointChange { get; set; }
        public string Reason { get; set; } = null!;
    }
}
