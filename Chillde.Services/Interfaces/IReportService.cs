using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Interfaces
{
    public interface IReportService
    {
        Task<ResponseModel> Reject(Guid reportId, ReportRejectModel reportRejectModel);
    }
}
