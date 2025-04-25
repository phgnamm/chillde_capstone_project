using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class VoucherUsageLogRepository : GenericRepository<VoucherUsageLog>, IVoucherUsageLogRepository
    {
        public VoucherUsageLogRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> CheckOrderHasUsedVoucher(Guid orderId, Guid accountId)
        {
            return _dbSet.Any(_ => _.OrderId == orderId && _.CreatedById == accountId);
        }

        public async Task<bool> CheckCustomerHasUsedVoucher(Guid voucherId, Guid accountId, Guid? serviceId)
        {
            var check = await _dbSet
                .Where(_ => _.VoucherId == voucherId && _.UsageStatus == Enums.UsageStatus.Used && _.CreatedById == accountId)
                .Include(_ => _.Order.Package)
                .FirstOrDefaultAsync();

            if (check == null)
                return false;

            if (serviceId != null)
            {
                if (check.Order?.Package?.ServiceId != serviceId)
                    return false;
            }

            return true;
        }

    }
}
