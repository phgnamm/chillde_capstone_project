using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Nest;

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
                        _.Status == Enums.OrderStatus.Completed);
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
            var orders = _dbSet.Where(_ => _.CreatedById == accountId && _.Package.Service.CreatedById == artistId && _.Status == Enums.OrderStatus.Completed && _.Stage == OrderStage.Completed).Include(_ => _.Package).ThenInclude(_ => _.Service).Count();
            return orders;
        }

        public async Task<IEnumerable<Order>> GetSketchOrdersWithResponseTimeAsync()
        {
            return await _dbSet
                .Where(_ => _.Status == OrderStatus.Accepted
                             && _.Stage == OrderStage.ReviewSketch && _.StartTime.HasValue && _.DeliveryTime.HasValue)
                .Include(_ => _.Package)
                .ThenInclude(_ => _.Service)
                .Include(_ => _.CreatedBy)
                    .ThenInclude(_ => _.Wallet)
                 .Include(_ => _.CreatedBy)
                 .ThenInclude(_ => _.AccountRoles).ThenInclude(_ => _.Role)
                .Include(_ => _.OrderTrackings)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrderToRemindDeadline()
        {
            var orders = await _dbSet
                         .Where(o => (o.Stage == OrderStage.SketchInProcess || o.Stage == OrderStage.ReviewSketch || o.Stage == OrderStage.DeliveryInProcess ) && o.Status == OrderStatus.Accepted && o.StartTime.HasValue && o.DeliveryTime.HasValue)
                         .Include(_ => _.Package)
                        .ThenInclude(_ => _.Service)
                        .ThenInclude(_ => _.CreatedBy).ThenInclude(_ => _.AccountRoles).ThenInclude(_ => _.Role)
                         .Include(_ => _.Package)
                        .ThenInclude(_ => _.Service)
                        .ThenInclude(_ => _.CreatedBy).ThenInclude(_ => _.AccountRoles).ThenInclude(_ => _.Reputations)
                        .Include(_ => _.CreatedBy)
                        .ThenInclude(_ => _.Wallet)
                        .Include(_ => _.CreatedBy)
                        .Include(_ => _.OrderTrackings)
                        .ToListAsync();
            return orders;
        
        }

        public async Task<int> NumberCompletedOrderOfArtisan(Guid artistId)
        {
            var orders = _dbSet.Where(_ => _.Package.Service.CreatedById == artistId && _.Status == Enums.OrderStatus.Completed).Include(_ => _.Package).ThenInclude(_ => _.Service).Count();
            return orders;
        }
    }
}
