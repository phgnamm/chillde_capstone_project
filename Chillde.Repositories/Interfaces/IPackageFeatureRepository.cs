using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IPackageFeatureRepository : IGenericRepository<PackageFeature>
    {
        Task<decimal> SumPriceOfExtraFeatures(List<Guid> ids);
        int CountAvailablePackageFeaturesByFeature(Guid featureId);
        int CountAvailablePackageFeaturesByPackage(Guid packageId);
    }
}
