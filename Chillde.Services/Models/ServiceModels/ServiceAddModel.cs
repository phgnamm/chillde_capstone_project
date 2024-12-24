using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceAddModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsOffter { get; set; }
        public ServiceStatus Status { get; set; }
    }
}
