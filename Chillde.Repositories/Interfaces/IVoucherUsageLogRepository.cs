using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Interfaces
{
    public interface IVoucherUsageLogRepository : IGenericRepository<VoucherUsageLog>
    {
        Task<bool> CheckOrderHasUsedVoucher(Guid orderId, Guid accountId);
        Task<bool> CheckCustomerHasUsedVoucher(Guid voucherId, Guid accountId);
    }
}
