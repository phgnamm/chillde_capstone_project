using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
