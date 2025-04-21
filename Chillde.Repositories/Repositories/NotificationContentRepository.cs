using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class NotificationContentRepository : GenericRepository<NotificationContent>, INotificationContentRepository
    {
        public NotificationContentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
        }

        public async Task<NotificationContent?> GetByKeyAsync(NotificationCode key)
        {
            if (!NotificationTypeCode.NotiCodes.TryGetValue(key, out string? code))
                return null;

            return await _dbSet.FirstOrDefaultAsync(x => x.Code!.Equals(code));
        }
    }
}
