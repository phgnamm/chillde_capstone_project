using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Models.CancellationReasonModels;
using Chillde.Services.Models.CancellationReasonModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface ICancellationReasonService
    {
        Task<ResponseModel> AddAsync(CancellationReasonAddModel model);
        Task<ResponseModel> UpdateAsync(Guid id, CancellationReasonAddModel model); 
        Task<ResponseModel> DeleteAsync(Guid id);
        Task<ResponseModel> GetAllAsync();
    }

}
