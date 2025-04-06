using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Report : BaseEntity
    {
        public string Description { get; set; }
        public ReportStatus Status { get; set; }
        // Foreign key
        public Guid OrderId { get; set; }

        // Relationship
        public Order Order { get; set; } = null!;
    }
}
