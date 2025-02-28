using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class RequestAttributeRepository : GenericRepository<RequestAttribute>, IRequestAttributeRepository
    {
        public RequestAttributeRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
