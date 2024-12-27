using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Services.Models.AccountModels;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface ICategoryServive
    {
        Task<ResponseModel> Add(CategoryAddModel categoryAddModel);
        Task<ResponseModel> AddList(CategoryAddRangeModel categoryAddModels);
        Task<ResponseModel> GetAll(CategoryFilterModel categoryFilterModel);
        Task<ResponseModel> Update(Guid id, CategoryUpdateModel categoryUpdateModel);
        Task<ResponseModel> Delete(Guid id);
        Task<ResponseModel> AddSubcategory(Guid categoryId, SubCategoryAddRangeModel subCategoryAddRangeModel);
        Task<ResponseModel> GetSubcategoriesByCategory(Guid categoryId, SubCategoryFilterModel subCategoryFilterModel);



    }
}
