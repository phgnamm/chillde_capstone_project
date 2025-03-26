using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class PackageRepository : GenericRepository<Package>, IPackageRepository
    {
        public PackageRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<Package> Get(Guid id)
        {
            return await _dbSet.Include(_ => _.PackageFeatures).ThenInclude(_ => _.Feature).FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<List<Package>> GetAllPackageFromService(Guid serviceId)
        {
            var result = await _dbSet.Where(_ => _.ServiceId == serviceId).ToListAsync();
            return result;
        }

        public async Task<List<Feature>> GetAllFeatureByService(Guid serviceId)
        {
            var result = await _dbSet.Where(_ => _.ServiceId == serviceId)
                                     .SelectMany(_ => _.PackageFeatures.Select(_ => _.Feature))
                                     .Where(_ => _.IsDeleted == false)
                                     .GroupBy(f => f.Name)
                                     .Select(g => g.First())
                                     .ToListAsync();
            return result;
        }

        public bool GetPackageByNameAsync(PackageName name, Guid serviceId)
        {
            var result =  _dbSet.Where(_ => _.ServiceId == serviceId && _.Name == name).Any();
            return result;
        }
    }
}
