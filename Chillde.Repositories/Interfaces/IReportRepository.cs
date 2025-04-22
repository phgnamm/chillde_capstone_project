using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Interfaces
{
    public interface IReportRepository : IGenericRepository<Report>
    {
        Task<Report> GetByOrder(Guid orderId);
    }
}
