using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Interfaces
{
    public interface ISystemConfigRepository : IGenericRepository<SystemConfig>
    {
        Task<SystemConfig?> GetByEntityTypeAsync(string fieldName);
        Task<SystemConfig?> GetByKeyAsync(SystemConfigKey key);
        Task<string?> GetValueByKeyAsync(SystemConfigKey key);
    }
}
