using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class FAQRepository : GenericRepository<FAQ>, IFAQRepository
    {
        public FAQRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
