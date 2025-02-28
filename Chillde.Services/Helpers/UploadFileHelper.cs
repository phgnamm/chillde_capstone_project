using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Common;
using Chillde.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Helpers
{
    public class UploadFileHelper
    {
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public UploadFileHelper(ICloudinaryHelper cloudinaryHelper)
        {
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<string> UploadFile(IFormFile fileUrl, string folderName)
        {
            if (fileUrl == null)
            {
                throw new ArgumentException("File URL cannot be null or empty.");
            }

            return await _cloudinaryHelper.UploadImageAsync(
                fileUrl,
                publicId: Guid.NewGuid().ToString(),
                folderName: folderName
            );
        }
    }

}
