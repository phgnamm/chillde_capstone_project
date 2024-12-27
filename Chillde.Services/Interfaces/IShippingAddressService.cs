using Chillde.Services.Common;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ShippingAddressModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IShippingAddressService
    {
        Task<ResponseModel> GetProvincesAsync(ProvinceFilterModel provinceFilterModel);
        Task<ResponseModel> GetDistrictsAsync(int provinceId);
        Task<ResponseModel> GetWardsAsync(int districtId);


    }
}
