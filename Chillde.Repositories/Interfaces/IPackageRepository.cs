using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IPackageRepository : IGenericRepository<Package>
    {
        Task<List<Package>> GetAllPackageFromService(Guid serviceId);
        Task<Package> Get(Guid id);
        //Task<Guid> GetArtist(Guid packageId);

    }
}
