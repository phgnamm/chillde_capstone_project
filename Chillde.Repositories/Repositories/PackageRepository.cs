using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool GetPackageByNameAsync(PackageName name, Guid serviceId)
        {
            var result =  _dbSet.Where(_ => _.ServiceId == serviceId && _.Name == name).Any();
            return result;
        }
    }
}
