using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class LanguageRepository : GenericRepository<Language>, ILanguageRepository
    {
        public LanguageRepository(AppDbContext context, IClaimService claimService)
            : base(context, claimService)
        {
        }
        public async Task<Language?> GetByCodeAsync(LanguageCode code)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.Code == code);
        }
    }
}
