using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;

namespace Chillde.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<ResponseModel> Add(VoucherAddModel voucherAddModel);
        Task<ResponseModel> Update(Guid id, VoucherUpdateModel voucherUpdateModel);
        Task<ResponseModel> StopVoucher(Guid id);
        Task<ResponseModel> GetAll(VoucherFilterModel voucherFilterModel);
    }
}
