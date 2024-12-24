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
    }
}
