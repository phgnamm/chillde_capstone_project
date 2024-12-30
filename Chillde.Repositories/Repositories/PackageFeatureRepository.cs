using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class PackageFeatureRepository : GenericRepository<PackageFeature>, IPackageFeatureRepository
    {
        public PackageFeatureRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
