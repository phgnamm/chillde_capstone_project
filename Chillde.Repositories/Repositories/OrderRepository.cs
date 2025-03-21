using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> HasCompletedOrder(Guid accountId, Guid serviceId)
        {
            var hasCompletedOrder = await _dbSet
                        .Include(_ => _.Package)
                        .AnyAsync(_ => _.Package.ServiceId == serviceId &&
                        _.CreatedBy.Id == accountId &&
                        _.Status == Enums.OrderStatus.Accepted);
            return hasCompletedOrder;
        }

        public async Task<bool> HasAnyOrderByService(Guid serviceId)
        {
            var hasCompletedOrder = _dbSet.Any(order => order.Package.ServiceId == serviceId);
            return hasCompletedOrder;
        }

        public async Task<bool> HasAnyOrderByPackage(Guid packageId)
        {
            var hasCompletedOrder = _dbSet.Any(order => order.PackageId == packageId);
            return hasCompletedOrder;
        }

        public async Task<bool> HasAnyOrderByFeature(Guid featureId)
        {
            //var hasCompletedOrder = _dbSet
            //    .Where(order => order.Package.PackageFeatures
            //    .Any(pf => pf.FeatureId == featureId))
            //    .Any();
            var hasCompletedOrder = _dbSet.Any(order => order.Package.PackageFeatures.Any(_ => _.FeatureId == featureId));
            return hasCompletedOrder;
        }

        public async Task<int> NumberCompletedOrder(Guid accountId, Guid artistId)
        {
            var orders = _dbSet.Where(_ => _.CreatedById == accountId && _.Package.Service.CreatedById == artistId && _.Status == Enums.OrderStatus.Success).Include(_ => _.Package).ThenInclude(_ => _.Service).Count();
            return orders;
        }
    }
}
