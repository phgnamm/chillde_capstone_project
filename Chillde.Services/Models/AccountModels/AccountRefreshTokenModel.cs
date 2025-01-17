namespace Chillde.Services.Models.AccountModels;

public class AccountRefreshTokenModel
{
    public Guid? DeviceId { get; set; }
    public string? AccessToken { get; set; }
    public Guid? RefreshToken { get; set; }
}