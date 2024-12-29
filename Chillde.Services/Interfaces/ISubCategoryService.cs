using Chillde.Services.Models.ItemModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ISubCategoryService
    {
        Task<ResponseModel> Update(Guid id, SubCategoryUpdateModel subCategoryUpdateModel);
        Task<ResponseModel> AddItem(Guid subcategoryId, ItemAddRangeModel itemAddRangeModel);
        Task<ResponseModel> GetItemBySubCategory(Guid subcategoryId, ItemFilterModel itemFilterModel);
        Task<ResponseModel> GetById(Guid id);

    }
}
