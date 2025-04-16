using Chillde.Services.Models.DepositModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IDepositService
    {
        Task<ResponseModel> GetAll(DepositFilterModel depositFilterModel);
    }
}
