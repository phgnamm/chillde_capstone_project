using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}