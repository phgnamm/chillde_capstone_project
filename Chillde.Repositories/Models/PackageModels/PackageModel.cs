using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.PackageModels
{
    public class PackageModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public Guid ServiceId { get; set; }
    }
}
