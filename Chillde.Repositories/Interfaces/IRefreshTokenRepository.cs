using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> FindByDeviceIdAsync(Guid deviceId);
}