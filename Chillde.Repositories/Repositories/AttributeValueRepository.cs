using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class AttributeValueRepository : GenericRepository<AttributeValue>, IAttributeValueRepository
    {
        public AttributeValueRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
