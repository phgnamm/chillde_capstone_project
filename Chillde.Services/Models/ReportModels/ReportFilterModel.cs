using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.ReportModels
{
    public class ReportFilterModel : FilterParameter
    {
        public Guid? OrderId { get; set; }
        public ReportStatus? Status { get; set; }
    }
}
