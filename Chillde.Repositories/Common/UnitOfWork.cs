using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Chillde.Repositories.Common;

public class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction _transaction;
    public UnitOfWork(AppDbContext context, 
        IAccountRepository accountRepository,
        IAccountConversationRepository accountConversationRepository,
        IAccountRoleRepository accountRoleRepository, IConversationRepository conversationRepository,
        IMessageRepository messageRepository, IMessageRecipientRepository messageRecipientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        IRequestRepository requestRepository,
        IRequestAttachmentRepository requestAttachmentRepository,
        IRequestAttributeAttachmentRepository requestAttributeAttachmentRepository,
        IRequestAttributeValueRepository requestAttributeValueRepository,
        IRequestAttributeRepository requestAttributeRepository,
        IServiceRepository serviceRepository,
        IFeedbackRepository feedbackRepository,
        IOrderRepository orderRepository,
        IPackageRepository packageRepository,
        IFeedbackAttachmentRepository feedbackAttachmnetRepository,
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        ITranslationRepository translationRepository,
        ILanguageRepository languageRepository,
        IServiceAttachmentRepository serviceAttachmentRepository,
        ICategoryRepository categoryRepository,
        IOfferRepository offerRepository,
        IFAQRepository faqRepository,
        IShippingAddressRepository shippingAddressRepository,
        IFeatureRepository featureRepository,
        IPackageFeatureRepository packageFeatureRepository,
        IServiceCollectionRepository serviceCollectionRepository,
        IServiceWishlistRepository serviceWishlistRepository,
        IShipmentRepository shipmentRepository,
        //IPaymentRepository paymentRepository,
        IUserActivityLogRepository userActivityLogRepository,
        ISystemConfigRepository systemConfigRepository,
        ISearchHistoryRepository searchHistoryRepository,
        IVoucherRepository voucherRepository,
        IVoucherUsageLogRepository voucherUsageLogRepository,
        ICancellationReasonRepository cancellationReasonRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IShipmentStatusHistoryRepository shipmentStatusHistoryRepository,
        IOfferAttachmentRepository offerAttachmentRepository,
        IReputationLogRepository reputationLogRepository
        )
    {
        Context = context;
        AccountRepository = accountRepository;
        AccountConversationRepository = accountConversationRepository;
        AccountRoleRepository = accountRoleRepository;
        ConversationRepository = conversationRepository;
        MessageRepository = messageRepository;
        MessageRecipientRepository = messageRecipientRepository;
        CancellationReasonRepository = cancellationReasonRepository;
        RefreshTokenRepository = refreshTokenRepository;
        RoleRepository = roleRepository;
        RequestRepository = requestRepository;
        RequestAttachmentRepository = requestAttachmentRepository;
        RequestAttributeAttachmentRepository = requestAttributeAttachmentRepository;
        RequestAttributeValueRepository = requestAttributeValueRepository;
        RequestAttributeRepository = requestAttributeRepository;
        ServiceRepository = serviceRepository;
        FeedbackRepository = feedbackRepository;
        OrderRepository = orderRepository;
        PackageRepository = packageRepository;
        FeedbackAttachmentRepository = feedbackAttachmnetRepository;
        TransactionRepository = transactionRepository;
        WalletRepository = walletRepository;
        TranslationRepository = translationRepository;
        LanguageRepository = languageRepository;
        ServiceAttachmentRepository = serviceAttachmentRepository;
        CategoryRepository = categoryRepository;
        FAQRepository = faqRepository;
        OfferRepository = offerRepository;
        ShippingAddressRepository = shippingAddressRepository;
        FeatureRepository = featureRepository;
        PackageFeatureRepository = packageFeatureRepository;
        ServiceCollectionRepository = serviceCollectionRepository;
        ServiceWishlistRepository = serviceWishlistRepository;
        ShipmentRepository = shipmentRepository;
        //PaymentRepository = paymentRepository;
        UserActivityLogRepository = userActivityLogRepository;
        SearchHistoryRepository = searchHistoryRepository;
        SystemConfigRepository = systemConfigRepository;
        VoucherRepository = voucherRepository;
        VoucherUsageLogRepository = voucherUsageLogRepository;
        OrderTrackingRepository = orderTrackingRepository;
        ShipmentStatusHistoryRepository = shipmentStatusHistoryRepository;
        OfferAttachmentRepository = offerAttachmentRepository;
        ReputationLogRepository = reputationLogRepository;

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
    public IServiceRepository ServiceRepository { get; }
    public IFeedbackRepository FeedbackRepository { get; }
    public IOrderRepository OrderRepository { get; }
    public IPackageRepository PackageRepository { get; }
    public IFeedbackAttachmentRepository FeedbackAttachmentRepository { get; }
    public IWalletRepository WalletRepository { get; }
    public ITransactionRepository TransactionRepository { get; }
    public ITranslationRepository TranslationRepository { get; }
    public ILanguageRepository LanguageRepository { get; }
    public IOfferRepository OfferRepository { get; }
    public IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IFAQRepository FAQRepository { get; }
    public IShippingAddressRepository ShippingAddressRepository { get; }
    public IServiceCollectionRepository ServiceCollectionRepository { get; }
    public IServiceWishlistRepository ServiceWishlistRepository { get; }
    public IFeatureRepository FeatureRepository { get; }
    public IPackageFeatureRepository PackageFeatureRepository { get; }
    public IShipmentRepository ShipmentRepository { get; }
    //public IPaymentRepository PaymentRepository { get; }
    public IUserActivityLogRepository UserActivityLogRepository { get; }
    public ISystemConfigRepository SystemConfigRepository { get; }

    public ISearchHistoryRepository SearchHistoryRepository { get; }

    public IRequestAttachmentRepository RequestAttachmentRepository { get; }

    public IRequestAttributeAttachmentRepository RequestAttributeAttachmentRepository{ get; }

    public IRequestAttributeValueRepository RequestAttributeValueRepository { get; }

    public IRequestAttributeRepository RequestAttributeRepository { get; }

    public IVoucherRepository VoucherRepository { get; }

    public IVoucherUsageLogRepository VoucherUsageLogRepository { get; }

    public IOrderTrackingRepository OrderTrackingRepository { get; }

    public ICancellationReasonRepository CancellationReasonRepository { get; }

    public IShipmentStatusHistoryRepository ShipmentStatusHistoryRepository { get; }
    public IOfferAttachmentRepository OfferAttachmentRepository { get; }

    public IReputationLogRepository ReputationLogRepository { get; }

    public async Task<int> SaveChangeAsync()
    {
        return await Context.SaveChangesAsync();
    }
    public async Task BeginTransactionAsync()
    {
        _transaction = await Context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
    }
}