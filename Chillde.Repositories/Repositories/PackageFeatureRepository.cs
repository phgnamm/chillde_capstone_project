using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<decimal> SumPriceOfExtraFeatures(List<Guid> ids)
        {
            return (decimal)await _dbSet.Where(_ => ids.Contains(_.Id) && _.IsExtra == true).SumAsync(_ => _.AdditionalCost);
        }

        public int CountAvailablePackageFeaturesByFeature(Guid featureId)
        {
            return _dbSet.Where(_ => _.FeatureId == featureId && _.IsDeleted == false).Count();
        }

        public int CountAvailablePackageFeaturesByPackage(Guid packageId)
        {
            return _dbSet.Where(_ => _.PackageId == packageId && _.IsDeleted == false).Count();
        }
    }
}
