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
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<bool> HasFeedback(Guid accountId, Guid serviceId)
        {
            var result = await _dbSet.AnyAsync(_ => _.CreatedById == accountId && _.ServiceId == serviceId);
            return result;

        }
    }
}
