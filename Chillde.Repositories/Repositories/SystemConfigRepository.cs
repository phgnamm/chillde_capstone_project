using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


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

        public async Task<SystemConfig?> GetByKeyAsync(SystemConfigKey key)
        {
            if (!SystemConfiguration.ConfigKeys.TryGetValue(key, out string? fieldName))
                return null;

            return await _dbSet.FirstOrDefaultAsync(x => x.FieldName == fieldName);
        }
    }
}
