using System.Security.Claims;
using Chillde.Repositories.Enums;
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

        var rolesClaim = identity?.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        if (rolesClaim != null && rolesClaim.Any())
        {
            GetCurrentRoles = new List<Role>();
            foreach (var role in rolesClaim)
            {
                if (Enum.TryParse(role.Value, out Role parsedRole))
                {
                    GetCurrentRoles.Add(parsedRole);
                }
            }
        }
    }

    public Guid? GetCurrentUserId { get; }
    public List<Role>? GetCurrentRoles { get; }
}