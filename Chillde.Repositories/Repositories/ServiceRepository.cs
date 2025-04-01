using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<List<Service>> GetTop14NewestServicesAsync()
        {
            return await _dbSet
                .OrderByDescending(s => s.CreationDate)
                .Take(14)
                .ToListAsync();
        }
    }
}
