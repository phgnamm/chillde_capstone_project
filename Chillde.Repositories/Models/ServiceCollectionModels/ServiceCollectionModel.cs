

using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ServiceCollectionModels
{
    public class ServiceCollectionModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}
