using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IReportService
    {
        Task<ResponseModel> Reject(Guid reportId, ReportRejectOrAcceptModel reportRejectModel);
        Task<ResponseModel> GetAll(ReportFilterModel reportFilterModel);
        Task<ResponseModel> GetStatusCount();
        Task<ResponseModel> Accept(Guid reportId, ReportRejectOrAcceptModel reportAcceptModel);
    }
}
