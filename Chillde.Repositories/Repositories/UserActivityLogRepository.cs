using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class UserActivityLogRepository : GenericRepository<UserActivityLog>, IUserActivityLogRepository 
    {
        private readonly AppDbContext _context;
        public UserActivityLogRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
            _context = context;
        }
    }
}
