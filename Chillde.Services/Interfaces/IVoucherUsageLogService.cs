using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherUsageLogModels;

namespace Chillde.Services.Interfaces
{
    public interface IVoucherUsageLogService
    {
        Task<ResponseModel> GetAll(VoucherUsageLogFilterModel voucherUsageLogFilterModel);
    }
}
