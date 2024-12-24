using Chillde.Services.Models.ResponseModels;
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


    }
}
