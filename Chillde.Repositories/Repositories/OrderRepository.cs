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

        public async Task<bool> HasAnyOrder(Guid serviceId)
        {
            var hasCompletedOrder = _dbSet.Any(order => order.Package.ServiceId == serviceId);
            return hasCompletedOrder;
        }
    }
}
