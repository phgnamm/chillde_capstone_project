using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories
{
    public class RequestAttributeAttachmentRepository : GenericRepository<RequestAttributeAttachment>, IRequestAttributeAttachmentRepository
    {
        public RequestAttributeAttachmentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }
    }
}
