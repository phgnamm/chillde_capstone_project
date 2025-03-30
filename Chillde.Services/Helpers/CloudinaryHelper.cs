using Chillde.Services.Interfaces;
using Chillde.Services.Utils;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Helpers;

public class CloudinaryHelper : ICloudinaryHelper
{
    private readonly ICloudinary _cloudinary;

    public CloudinaryHelper(ICloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string? name = null, string? publicId = null,
        bool? overwrite = true, string? folderName = null)
    {
        var parameters = new ImageUploadParams
        {
            File = new FileDescription(name ?? file.FileName, file.OpenReadStream()),
            PublicId = publicId ?? AuthenticationTools.GenerateUniqueToken(),
            Overwrite = overwrite,
            Folder = folderName,
        };

        var result = await _cloudinary.UploadAsync(parameters);
        return result.SecureUrl.ToString();
    }

    public async Task<string> RemoveImagesAsync(List<string> publicIds)
    {
        if (publicIds == null || publicIds.Count == 0)
        {
            return "No images to delete.";
        }

        var deletionParams = new DelResParams()
        {
            PublicIds = publicIds,
            Type = "upload",
            ResourceType = ResourceType.Image
        };

        var result = await _cloudinary.DeleteResourcesAsync(deletionParams);

        return $"Deleted {result.Deleted.Count} images successfully.";
    }

}