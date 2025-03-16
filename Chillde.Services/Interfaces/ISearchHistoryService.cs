using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface ISearchHistoryService
    {
        Task<ResponseModel> Delete(Guid id);
    }
}
