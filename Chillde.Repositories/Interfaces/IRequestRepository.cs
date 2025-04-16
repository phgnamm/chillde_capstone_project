using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IRequestRepository : IGenericRepository<Request>
    {
        Task<bool> HasUserOfferedForRequestAsync(Guid currentUserId, Guid requestId);
    }
}
