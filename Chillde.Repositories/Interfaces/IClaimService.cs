using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Interfaces;

public interface IClaimService
{
    public Guid? GetCurrentUserId { get; }
    public List<Role>? GetCurrentRoles { get; }
}