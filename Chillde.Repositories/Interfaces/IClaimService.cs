namespace Chillde.Repositories.Interfaces;

public interface IClaimService
{
    public Guid? GetCurrentUserId { get; }
}