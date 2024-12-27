using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Chillde.Repositories.Common;

public class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction _transaction;
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
        ISubCategoryRepository subCategoryRepository,
        IItemRepository itemRepository,
        IOfferRepository offerRepository,
        IFAQRepository faqRepository,
        IAttributeRepository attributeRepository,
        IAttributeValueRepository attributeValueRepository,
        IItemAttributeRepository itemAttributeRepository,
        IShippingAddressRepository shippingAddressRepository
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
        SubCategoryRepository = subCategoryRepository;
        ItemRepository = itemRepository;
        OfferRepository = offerRepository;
        AttributeRepository = attributeRepository;
        AttributeValueRepository = attributeValueRepository;
        ItemAttributeRepository = itemAttributeRepository;
        ShippingAddressRepository = shippingAddressRepository;
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

    public IServiceRepository ServiceRepository { get; }

    public IFeedbackRepository FeedbackRepository { get; }

    public IOrderRepository OrderRepository { get; }

    public IPackageRepository PackageRepository { get; }

    public IFeedbackAttachmentRepository FeedbackAttachmentRepository { get; }

    public IWalletRepository WalletRepository { get; }

    public IWalletHistoryRepository WalletHistoryRepository { get; }
    public ITranslationRepository TranslationRepository { get; }
    public ILanguageRepository LanguageRepository { get; }
    public IOfferRepository OfferRepository { get; }
    public IServiceAttachmentRepository ServiceAttachmentRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IFAQRepository FAQRepository { get; }
    public ISubCategoryRepository SubCategoryRepository { get; }
    public IItemRepository ItemRepository { get; }
    public IShippingAddressRepository ShippingAddressRepository { get; }

    public IItemAttributeRepository ItemAttributeRepository { get; }

    public IAttributeRepository AttributeRepository {get;}

    public IAttributeValueRepository AttributeValueRepository {get;}

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