namespace Chillde.Services.Models.TokenModels;

public class TokenModel
{
    public Guid DeviceId { get; set; }
    public string AccessToken { get; set; } = null!;
    public Guid RefreshToken { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}