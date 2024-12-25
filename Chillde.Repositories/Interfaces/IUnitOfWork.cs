namespace Chillde.Repositories.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }

    public Task<int> SaveChangeAsync();
    Task BeginTransactionAsync(); 
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    #region Repository

    IAccountRepository AccountRepository { get; }
    IAccountConversationRepository AccountConversationRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IConversationRepository ConversationRepository { get; }
    IMessageRepository MessageRepository { get; }
    IMessageRecipientRepository MessageRecipientRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IRoleRepository RoleRepository { get; }
    IRequestRepository RequestRepository { get; }
    IRequestDetailRepository RequestDetailRepository { get; }
    IServiceRepository ServiceRepository { get; }
    IFeedbackRepository FeedbackRepository { get; }
    IOrderRepository OrderRepository { get; }
    IPackageRepository PackageRepository { get; }
    IFeedbackAttachmentRepository FeedbackAttachmentRepository { get; }
    IWalletRepository WalletRepository { get; }
    IWalletHistoryRepository WalletHistoryRepository { get; }
    ITranslationRepository TranslationRepository { get; }
    ILanguageRepository LanguageRepository { get; }
    IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    IOfferRepository OfferRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IFAQRepository FAQRepository { get; }
    ISubCategoryRepository SubCategoryRepository { get; }
    IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    IItemRepository ItemRepository { get; }

    #endregion
}