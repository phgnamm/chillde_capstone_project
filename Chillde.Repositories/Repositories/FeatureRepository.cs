using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using AutoMapper.Features;

namespace Chillde.Repositories.Repositories
{
    public class FeatureRepository : GenericRepository<Feature>, IFeatureRepository
    {
        public FeatureRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> HasAnyOrderByFeature(Guid featureId)
        {
            var hasCompletedOrder = _dbSet
                .Where(f => f.Id == featureId)
                .SelectMany(f => f.PackageFeatures)
                .Select(_ => _.Package)
                .SelectMany(pf => pf.Orders)
                .Count();
            return hasCompletedOrder > 0;
        }
    }
}
