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
        IRequestRepository requestRepository,
        IServiceRepository serviceRepository,
        IFeedbackRepository feedbackRepository,
        IOrderRepository orderRepository,
        IPackageRepository packageRepository,
        IFeedbackImageRepository feedbackImageRepository,
        IWalletHistoryRepository walletHistoryRepository,
        IWalletRepository walletRepository)
        IRoleRepository roleRepository, ITranslationRepository translationRepository,
        ILanguageRepository languageRepository)
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
        ServiceRepository = serviceRepository;
        FeedbackRepository = feedbackRepository;
        OrderRepository = orderRepository;
        PackageRepository = packageRepository;
        FeedbackImageRepository = feedbackImageRepository;
        WalletHistoryRepository = walletHistoryRepository;
        WalletRepository = walletRepository;
        TranslationRepository = translationRepository;
        LanguageRepository = languageRepository;
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

    public IServiceRepository ServiceRepository {  get; }

    public IFeedbackRepository FeedbackRepository { get; }

    public IOrderRepository OrderRepository { get; }

    public IPackageRepository PackageRepository {get ;}

    public IFeedbackImageRepository FeedbackImageRepository {  get; }

    public IWalletRepository WalletRepository {  get; }

    public IWalletHistoryRepository WalletHistoryRepository {  get; }
    public ITranslationRepository TranslationRepository { get; }
    public ILanguageRepository LanguageRepository { get; }

    public async Task<int> SaveChangeAsync()
    {
        return await Context.SaveChangesAsync();
    }
}