using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IPackageService
    {
        Task<ResponseModel> Update(PackageUpdateModel packageUpdateModel, Guid id);
    }
}
