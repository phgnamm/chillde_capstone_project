using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Repositories;

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
        IFeedbackAttachmentRepository feedbackAttachmnetRepository,
        IWalletHistoryRepository walletHistoryRepository,
        IWalletRepository walletRepository,
        ITranslationRepository translationRepository,
        ILanguageRepository languageRepository,
        IServiceAttachmentRepository serviceAttachmentRepository,
        ICategoryRepository categoryRepository,
        IFAQRepository faqRepository
        )
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
        FeedbackAttachmentRepository = feedbackAttachmnetRepository;
        WalletHistoryRepository = walletHistoryRepository;
        WalletRepository = walletRepository;
        TranslationRepository = translationRepository;
        LanguageRepository = languageRepository;
        ServiceAttachmentRepository = serviceAttachmentRepository;
        CategoryRepository = categoryRepository;
        FAQRepository = faqRepository;
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

    public IFeedbackAttachmentRepository FeedbackAttachmentRepository {  get; }

    public IWalletRepository WalletRepository {  get; }

    public IWalletHistoryRepository WalletHistoryRepository {  get; }
    public ITranslationRepository TranslationRepository { get; }
    public ILanguageRepository LanguageRepository { get; }
    public IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IFAQRepository FAQRepository { get; }

    public async Task<int> SaveChangeAsync()
    {
        return await Context.SaveChangesAsync();
    }
}