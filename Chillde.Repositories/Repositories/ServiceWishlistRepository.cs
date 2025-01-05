using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class ServiceWishlistRepository : GenericRepository<ServiceWishlist>, IServiceWishlistRepository
    {
        public ServiceWishlistRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
