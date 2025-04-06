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
            entity.Property(account => account.WalletId).IsRequired();
            entity.HasIndex(account => account.Username).IsUnique();
            entity.HasIndex(account => account.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(order => order.Code).IsUnique();
            entity.HasIndex(order => order.ShipmentCode).IsUnique();
            entity.Property(order => order.Phone).HasMaxLength(15);
            entity.Property(order => order.Address).HasMaxLength(256);
            entity.Property(order => order.CreatedById).IsRequired();
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.Property(request => request.Name).HasMaxLength(100);
            entity.Property(request => request.CreatedById).IsRequired();
        });
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(languague => languague.Code).IsUnique();
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
            entity.HasIndex(voucher => voucher.Slug).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<FAQ>(entity => { entity.Property(faq => faq.Question).HasMaxLength(100); });

        modelBuilder.Entity<Feature>(entity => { entity.Property(feature => feature.Name).HasMaxLength(100); });

        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.Property(orderTracking => orderTracking.Name).HasMaxLength(100);
            entity.Property(orderTracking => orderTracking.CreatedById).IsRequired();
        });

        modelBuilder.Entity<Package>(entity => { entity.Property(package => package.Name).HasMaxLength(100); });

        modelBuilder.Entity<ServiceCollection>(entity =>
        {
            entity.Property(serviceCollection => serviceCollection.Name).HasMaxLength(100);
            entity.Property(serviceCollection => serviceCollection.CreatedById).IsRequired();
        });

        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.Property(shippingAddress => shippingAddress.FullName).HasMaxLength(50);
            entity.Property(shippingAddress => shippingAddress.PhoneNumber).HasMaxLength(15);
            entity.Property(shippingAddress => shippingAddress.WardCode).HasMaxLength(50);
        });

        modelBuilder.Entity<Translation>(entity =>
        {
            entity.Property(translation => translation.FieldName).HasMaxLength(50);
            entity.Property(translation => translation.EntityType).HasMaxLength(50);
        });

        modelBuilder.Entity<Message>(entity => { entity.Property(message => message.CreatedById).IsRequired(); });
        modelBuilder.Entity<Service>(entity => { entity.Property(service => service.CreatedById).IsRequired(); });
        modelBuilder.Entity<Offer>(entity => { entity.Property(offer => offer.CreatedById).IsRequired(); });
        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.Property(shippingAddress => shippingAddress.CreatedById).IsRequired();
        });
        modelBuilder.Entity<Feedback>(entity => { entity.Property(feedback => feedback.CreatedById).IsRequired(); });
        modelBuilder.Entity<SystemConfig>(entity => { entity.Property(e => e.Value).HasColumnType("jsonb"); });
        modelBuilder.Entity<AccountRole>(entity => { entity.Property(e => e.TotalReputation).HasDefaultValue(100); });
        modelBuilder.Entity<Package>(entity => { entity.Property(e => e.Name).HasColumnType("int"); });
        modelBuilder.Entity<Report>();
        #endregion

        #region Relationship Configuration

        modelBuilder.Entity<Shipment>()
            .HasOne(a => a.Order)
            .WithMany(w => w.Shipments)
            .HasForeignKey(a => a.OrderId);
        modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(t => new { t.EntityType, t.EntityId, t.FieldName, t.LanguageId });

            entity.HasOne(t => t.Language)
                .WithMany(l => l.Translations)
                .HasForeignKey(t => t.LanguageId);
        });
        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasIndex(voucher => voucher.Code).IsUnique();
            entity.HasOne(v => v.Creator)
            .WithMany(a => a.CreatedVouchers)
            .HasForeignKey(v => v.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Voucher>()
            .HasOne(v => v.Receiver)
            .WithMany(a => a.ReceivedVouchers)
            .HasForeignKey(v => v.ReceiverId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServiceWishlist>(entity =>
        {
            entity.HasKey(sw => new { sw.ServiceId, sw.ServiceCollectionId });
            entity.Ignore(sw => sw.Id);
        });
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(f => f.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Artisan)
            .WithMany()
            .HasForeignKey(f => f.ArtisanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasOne(s => s.Service)
                .WithMany(c => c.Packages);
        });

        modelBuilder.Entity<PackageFeature>(entity =>
        {
            entity.HasOne(pf => pf.Package)
    .WithMany(p => p.PackageFeatures)
    .HasForeignKey(pf => pf.PackageId)
    .OnDelete(DeleteBehavior.Cascade);
        });



        #endregion
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
    public DbSet<FeedbackAttachment> FeedbackAttachments { get; set; }
    public DbSet<RequestAttribute> RequestAttributes { get; set; }
    public DbSet<RequestAttributeAttachment> RequestAttributeAttachments { get; set; }
    public DbSet<RequestAttributeValue> RequestAttributeValues { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Order> Orders { get; set; }
    //public DbSet<Payment> Payments { get; set; }
    public DbSet<OrderInformation> OrderInformation { get; set; }
    public DbSet<OrderInformationAttachment> OrderInformationAttachments { get; set; }
    public DbSet<OrderTracking> OrderTrackings { get; set; }
    public DbSet<OrderTrackingAttachment> OrderTrackingAttachments { get; set; }
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageFeature> PackageFeatures { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceCollection> ServiceCollection { get; set; }
    public DbSet<ServiceAttachment> ServiceAttachments { get; set; }
    public DbSet<ServiceWishlist> ServiceWishlists { get; set; }
    public DbSet<Shipment> Shipment { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }
    public DbSet<ProductShipment> ProductShipment { get; set; }
    public DbSet<ReputationLog> ReputationLogs { get; set; }
    public DbSet<CancellationReason> CancellationReasons { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherUsageLog> VoucherUsageLogs { get; set; }
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistory { get; set; }

    #endregion
}