using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Interfaces;

public interface ICloudinaryHelper
{
    Task<string> UploadImageAsync(IFormFile file, string? name = null, string? publicId = null, bool? overwrite = true, string? folderName = null);
}