using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<Wallet> GetWalletByAccount(Guid accountId)
        {
            var walletOfUser = await _dbSet.Where(_ => _.CreatedById == accountId).FirstOrDefaultAsync();
            return walletOfUser;
        }
    }
}
