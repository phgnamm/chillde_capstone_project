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
    IOfferRepository OfferRepository { get; }
    ITranslationRepository TranslationRepository { get; }
    ILanguageRepository LanguageRepository { get; }
    IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IFAQRepository FAQRepository { get; }
    ISubCategoryRepository SubCategoryRepository { get; }
    IItemRepository ItemRepository { get; }
    IItemAttributeRepository ItemAttributeRepository { get; }
    IAttributeRepository AttributeRepository { get; }
    IAttributeValueRepository AttributeValueRepository { get; }
    IShippingAddressRepository ShippingAddressRepository { get; }
    IFeatureRepository FeatureRepository { get; }
    IPackageFeatureRepository PackageFeatureRepository { get; }
    IServiceCollectionRepository ServiceCollectionRepository { get; }
    IServiceWishlistRepository ServiceWishlistRepository { get; }
    IShipmentRepository ShipmentRepository { get; }
    IPaymentRepository PaymentRepository { get; }

    IUserActivityLogRepository UserActivityLogRepository { get; }

    #endregion
}