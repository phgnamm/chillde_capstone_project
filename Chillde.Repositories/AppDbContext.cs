using Chillde.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories;
    public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Fix for PostgreSQL timestamp with time zone issues
        // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        // AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    #region DbSets

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountConversation> AccountConversations { get; set; }
    public DbSet<AccountRole> AccountRoles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FeedbackImage> FeedbackImages { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemAttribute> ItemAttributes { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderInformation> OrderInformation { get; set; }
    public DbSet<OrderInformationAttachment> OrderInformationAttachments { get; set; }
    public DbSet<OrderTracking> OrderTrackings { get; set; }
    public DbSet<OrderTrackingImage> OrderTrackingImages { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageFeature> PackageFeatures { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestDetail> RequestDetail { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceCollection> ServiceCollection { get; set; }
    public DbSet<ServiceImage> ServiceImages { get; set; }
    public DbSet<ServiceWishlist> ServiceWishlists { get; set; }
    public DbSet<Shipment> Shipment { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletHistory> WalletHistory { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region Entity Properties Configuration

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(account => account.FirstName).HasMaxLength(50);
            entity.Property(account => account.LastName).HasMaxLength(50);
            entity.Property(account => account.Username).HasMaxLength(50);
            entity.Property(account => account.Email).HasMaxLength(256);
            entity.Property(account => account.PhoneNumber).HasMaxLength(15);
            entity.HasIndex(account => account.Username).IsUnique();
            entity.HasIndex(account => account.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.Phone).HasMaxLength(15);
            entity.Property(order => order.Address).HasMaxLength(256);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.Property(request => request.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(role => role.Name).HasMaxLength(50);
            entity.Property(role => role.Description).HasMaxLength(256);
            entity.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(conversation => conversation.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name).HasMaxLength(100);
            entity.Property(category => category.Code).HasMaxLength(25);
        });

        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.Property(faq => faq.Question).HasMaxLength(100);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.Property(feature => feature.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(100);
            entity.Property(item => item.Code).HasMaxLength(25);
        });

        modelBuilder.Entity<ItemAttribute>(entity =>
        {
            entity.Property(itemAttribute => itemAttribute.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.Property(orderTracking => orderTracking.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.Property(package => package.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ServiceCollection>(entity =>
        {
            entity.Property(serviceCollection => serviceCollection.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(shipment => shipment.Code).HasMaxLength(50);
        });

        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.Property(shippingAddress => shippingAddress.FullName).HasMaxLength(50);
            entity.Property(shippingAddress => shippingAddress.PhoneNumber).HasMaxLength(15);
            entity.Property(shippingAddress => shippingAddress.WardCode).HasMaxLength(50);
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.Property(subCategory => subCategory.Code).HasMaxLength(50);
            entity.Property(subCategory => subCategory.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Translation>(entity =>
        {
            entity.Property(translation => translation.FieldName).HasMaxLength(50);
            entity.Property(translation => translation.EntityType).HasMaxLength(50);
        });

        #endregion

        #region Relationship Configuration

        modelBuilder.Entity<Wallet>()
             .HasOne(o => o.CreatedBy)
             .WithMany()
             .HasForeignKey(o => o.CreatedById)
             .IsRequired(true);

        modelBuilder.Entity<Message>()
             .HasOne(o => o.CreatedBy)
             .WithMany()
             .HasForeignKey(o => o.CreatedById)
             .IsRequired(true);

        modelBuilder.Entity<Shipment>()
            .HasOne(a => a.Order)
            .WithOne(w => w.Shipment)
            .HasForeignKey<Order>(w => w.ShipmentId);

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasOne(s => s.Account)
                .WithMany(a => a.Skills)
                .HasForeignKey(s => s.AccountId);

            entity.HasOne(s => s.SubCategory)
                .WithMany(sc => sc.Skills)
                .HasForeignKey(s => s.SubCategoryId);
        });


        modelBuilder.Entity<Translation>()
            .HasOne(t => t.Language)
            .WithMany(l => l.Translations)
            .HasForeignKey(t => t.LanguageId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .IsRequired(true);

        modelBuilder.Entity<OrderTracking>()
           .HasOne(o => o.CreatedBy)
           .WithMany()
           .HasForeignKey(o => o.CreatedById)
           .IsRequired(true);

        modelBuilder.Entity<Service>()
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .IsRequired(true);

        modelBuilder.Entity<Offer>()
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .IsRequired(true);

        modelBuilder.Entity<Request>()
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .IsRequired(true);

        #endregion
    }
}