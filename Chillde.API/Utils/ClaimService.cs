using System.Security.Claims;
using Chillde.Repositories.Interfaces;

namespace Chillde.API.Utils;

public class ClaimService : IClaimService
{
    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var accountIdClaim = identity?.FindFirst("accountId");
        if (accountIdClaim != null && Guid.TryParse(accountIdClaim.Value, out var currentUserId))
            GetCurrentUserId = currentUserId;
    }

    public Guid? GetCurrentUserId { get; }
}