using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace Chillde.Repositories.Repositories
{
    public class PackageFeatureRepository : GenericRepository<PackageFeature>, IPackageFeatureRepository
    {
        public PackageFeatureRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
