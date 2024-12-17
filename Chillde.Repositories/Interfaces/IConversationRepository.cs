using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Interfaces;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<Conversation?> FindByAccountIdsAsync(List<Guid> accountIds);

    Task<Conversation?> FindByAccountIdAndConversationIdAsync(Guid accountId, Guid conversationId,
        Func<IQueryable<Conversation>, IQueryable<Conversation>>? include = null);
}