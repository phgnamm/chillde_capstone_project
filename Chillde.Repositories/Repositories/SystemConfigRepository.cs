using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Chillde.Repositories.Repositories
{
    public class SystemConfigRepository : GenericRepository<SystemConfig>, ISystemConfigRepository
    {
        public SystemConfigRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
      
        public async Task<SystemConfig?> GetByEntityTypeAsync(string fieldName)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.FieldName == fieldName);
        }
    }
}
