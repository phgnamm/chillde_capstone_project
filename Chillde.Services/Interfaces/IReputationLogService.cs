using Chillde.Services.Models.ReputationLogModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IReputationLogService
    {
        Task<ResponseModel> GetAll(ReputationLogFilterModel reputationLogFilterModel);
    }
}
