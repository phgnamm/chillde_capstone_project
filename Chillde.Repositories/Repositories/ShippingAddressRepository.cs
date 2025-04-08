using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class ShippingAddressRepository : GenericRepository<ShippingAddress>, IShippingAddressRepository
    {
        public ShippingAddressRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {

        }

        public async Task<ShippingAddress?> FindDefaultShippingAddressByAccountIdAsync(Guid accountId)
        {
            return  await _dbSet.FirstOrDefaultAsync(x => x.IsDefault && x.CreatedById == accountId);
        }
    }
}
