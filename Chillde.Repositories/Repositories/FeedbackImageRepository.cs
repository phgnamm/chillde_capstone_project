using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class FeedbackImageRepository : GenericRepository<FeedbackImage>, IFeedbackImageRepository
    {
        public  FeedbackImageRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }


    }
}
