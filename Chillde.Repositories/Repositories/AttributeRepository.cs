using Chillde.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class AttributeRepository : GenericRepository<Entities.Attribute>, IAttributeRepository
    {
        public AttributeRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
