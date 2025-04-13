using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class VoucherUsageLogRepository : GenericRepository<VoucherUsageLog>, IVoucherUsageLogRepository
    {
        public VoucherUsageLogRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> CheckOrderHasUsedVoucher(Guid orderId, Guid voucherId)
        {
            return _dbSet.Any(_ => _.OrderId == orderId && _.VoucherId == voucherId);
        }

        public async Task<bool> CheckOrderHasUsedVoucherForCustomer(Guid orderId, Guid voucherId)
        {
            return _dbSet.Any(_ => _.OrderId == orderId && _.VoucherId == voucherId && _.UsageStatus == Enums.UsageStatus.Used);

        }
    }
}
