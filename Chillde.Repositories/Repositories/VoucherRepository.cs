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
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<List<Voucher>> CheckHasVoucher(Guid artistId)
        {
            var result = await _dbSet
                         .Where(_ => _.CreatedById == artistId
                                           && _.ExpiredTime >= DateTime.UtcNow 
                                           && _.VoucherStatus == Enums.VoucherStatus.Pending
                                           && (!_.RemainingQuantity.HasValue || _.RemainingQuantity > 0)) 
                         .ToListAsync();

            return result;
        }
    }
}
