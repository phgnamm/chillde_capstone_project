using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Interfaces
{
    public interface IServiceAttachmentRepository : IGenericRepository<ServiceAttachment>
    {
        Task<List<ServiceAttachment>> GetAllAsync(Guid serviceId);
    }
}

