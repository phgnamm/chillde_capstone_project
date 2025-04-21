using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<Report> GetByOrder(Guid orderId)
        {
            var report = _dbSet.Where(_ => _.OrderId == orderId && _.IsDeleted == false).FirstOrDefault();
            return report;
        }
    }
}
