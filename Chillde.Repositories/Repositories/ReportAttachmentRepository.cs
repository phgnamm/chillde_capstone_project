using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class ReportAttachmentRepository : GenericRepository<ReportAttachment>, IReportAttachmentRepository
    {
        public ReportAttachmentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
