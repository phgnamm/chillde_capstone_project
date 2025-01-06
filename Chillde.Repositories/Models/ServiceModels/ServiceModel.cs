using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.ServiceModels
{
    public class ServiceModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsOffter { get; set; }
        public ServiceStatus Status { get; set; }
    }
}
