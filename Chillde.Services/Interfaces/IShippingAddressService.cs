using Chillde.Services.Common;
using Chillde.Services.Models.CategoryModels;
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
        Task<ResponseModel> GetProvincesAsync();
        Task<ResponseModel> GetDistrictsAsync(int provinceId);
        Task<ResponseModel> GetWardsAsync(int districtId);
        Task<ResponseModel> AddShippingAddressAsync(ShippingAddressAddModel request);
        Task<ResponseModel> GetAllAsync(Guid AccountId, ShippingAddressFilterModel shippingAddressFilterModel);   
        Task<ResponseModel> GetByIdAsync(Guid id);
        Task<ResponseModel> UpdateShippingAddressAsync(Guid id, ShippingAddressUpdateModel request);
        Task<ResponseModel> Delete(Guid id);




    }
}
