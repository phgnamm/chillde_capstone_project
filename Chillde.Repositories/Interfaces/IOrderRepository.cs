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
        Task<bool> HasAnyOrder(Guid serviceId);
    }
}
