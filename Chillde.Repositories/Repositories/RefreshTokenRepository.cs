using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<RefreshToken?> FindByDeviceIdAsync(Guid deviceId)
    {
        return await _dbSet.FirstOrDefaultAsync(refreshToken => refreshToken.DeviceId == deviceId);
    }
}