
using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.ServiceWishlistModels
{
    public class ServiceWishlistModel : BaseEntity
    {
        public required Guid ServiceId { get; set; }
        public required string ServiceName { get; set; }
    }
}
