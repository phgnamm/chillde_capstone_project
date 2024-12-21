using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Common;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(AppDbContext context, IAccountRepository accountRepository,
        IAccountConversationRepository accountConversationRepository,
        IAccountRoleRepository accountRoleRepository, IConversationRepository conversationRepository,
        IMessageRepository messageRepository, IMessageRecipientRepository messageRecipientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        IRequestDetailRepository requestDetailRepository,
        IRequestRepository requestRepository)
    {
        Context = context;
        AccountRepository = accountRepository;
        AccountConversationRepository = accountConversationRepository;
        AccountRoleRepository = accountRoleRepository;
        ConversationRepository = conversationRepository;
        MessageRepository = messageRepository;
        MessageRecipientRepository = messageRecipientRepository;
        RefreshTokenRepository = refreshTokenRepository;
        RoleRepository = roleRepository;
        RequestDetailRepository = requestDetailRepository;
        RequestRepository = requestRepository;
    }

    public AppDbContext Context { get; }
    public IAccountRepository AccountRepository { get; }
    public IAccountConversationRepository AccountConversationRepository { get; }
    public IAccountRoleRepository AccountRoleRepository { get; }
    public IConversationRepository ConversationRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public IMessageRecipientRepository MessageRecipientRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    public IRoleRepository RoleRepository { get; }
    public IRequestRepository RequestRepository { get; }
    public IRequestDetailRepository RequestDetailRepository { get; }

    public async Task<int> SaveChangeAsync()
    {
        return await Context.SaveChangesAsync();
    }
}