using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class ServiceAttachmentRepository : GenericRepository<ServiceAttachment>, IServiceAttachmentRepository
    {
        public ServiceAttachmentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<List<ServiceAttachment>> GetAllAsync(Guid serviceId)
        {
            var result = await _dbSet.Where(_ => _.ServiceId == serviceId).ToListAsync();
            return result;
        }
    }
}

