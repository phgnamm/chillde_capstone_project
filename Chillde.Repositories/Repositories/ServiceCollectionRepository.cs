using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;


namespace Chillde.Repositories.Repositories
{
    public class ServiceCollectionRepository : GenericRepository<ServiceCollection>, IServiceCollectionRepository
    {
        public ServiceCollectionRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
