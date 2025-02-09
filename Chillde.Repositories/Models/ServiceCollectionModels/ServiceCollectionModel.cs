

using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.AccountModels;

namespace Chillde.Repositories.Models.ServiceCollectionModels
{
    public class ServiceCollectionModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public AccountLiteModel CreatedBy { get; set; } = null!;
    }
}
