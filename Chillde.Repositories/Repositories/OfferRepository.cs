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
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        public OfferRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> RequestHasOffered(Guid requestId)
        {
            var result = await _dbSet.AnyAsync(_ => _.RequestId == requestId);
            return result;
        }
    }
}
