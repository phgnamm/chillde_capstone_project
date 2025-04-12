using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<bool> HasCompletedOrder(Guid accountId, Guid serviceId);
        Task<int> NumberCompletedOrder(Guid accountId, Guid artistId);
        Task<bool> HasAnyOrderByService(Guid serviceId);
        Task<bool> HasAnyOrderByPackage(Guid packageId);
        Task<bool> HasAnyOrderByFeature(Guid featureId);
        Task<IEnumerable<Order>> GetSketchOrdersWithResponseTimeAsync();
        Task<IEnumerable<Order>> GetOrderToRemindDeadline();

    }
}
