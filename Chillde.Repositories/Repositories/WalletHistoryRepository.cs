using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class WalletHistoryRepository : GenericRepository<WalletHistory>, IWalletHistoryRepository
    {
        public WalletHistoryRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }


    }
}
