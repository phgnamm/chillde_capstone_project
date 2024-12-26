using Chillde.Services.Helpers;
using Chillde.Services.Models.ItemModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IItemService
    {
        Task<ResponseModel> GetById(Guid itemId);

        Task<ResponseModel> Update(Guid id, ItemUpdateModel itemUpdateModel);

    }
}
