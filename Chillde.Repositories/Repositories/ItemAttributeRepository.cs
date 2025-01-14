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
    public class ItemAttributeRepository : GenericRepository<ItemAttribute>, IItemAttributeRepository
    {
        public ItemAttributeRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<ItemAttribute> FirstOrDefaultAsync(Guid itemId, Guid attributeId)
        {
            return await _dbSet.Include(_ => _.Attribute).Include(_ => _.Item).FirstOrDefaultAsync(_ => _.ItemId == itemId && _.AttributeId == attributeId);
        }
    }
}
