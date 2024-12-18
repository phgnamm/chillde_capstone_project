using Chillde.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // These lines will fix the error "Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported"
        // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        // AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountConversation> AccountConversations { get; set; }
    public DbSet<AccountRole> AccountRoles { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Request> Requests { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(account => account.FirstName).HasMaxLength(50);
            entity.Property(account => account.LastName).HasMaxLength(50);
            entity.HasIndex(account => account.Username).IsUnique();
            entity.Property(account => account.Username).HasMaxLength(50);
            entity.HasIndex(account => account.Email).IsUnique();
            entity.Property(account => account.Email).HasMaxLength(256);
            entity.Property(account => account.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Account>()
        .HasOne(a => a.Wallet)
        .WithOne(w => w.Account)
        .HasForeignKey<Wallet>(w => w.AccountId);

        modelBuilder.Entity<Shipment>()
        .HasOne(a => a.Order)
        .WithOne(w => w.Shipment)
        .HasForeignKey<Order>(w => w.ShipmentId);

        modelBuilder.Entity<Skill>()
        .HasOne(s => s.Account)
        .WithMany(a => a.Skills)
        .HasForeignKey(s => s.AccountId);

        modelBuilder.Entity<Skill>()
        .HasOne(s => s.SubCategory)
        .WithMany()
        .HasForeignKey(s => s.SubCategoryId);

        modelBuilder.Entity<Translation>()
        .HasOne(t => t.Language)
        .WithMany(l => l.Translations)
        .HasForeignKey(t => t.LanguageId);

        modelBuilder.Entity<Order>()
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

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(conversation => conversation.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(role => role.Name).HasMaxLength(50);
            entity.HasIndex(role => role.Name).IsUnique();
            entity.Property(role => role.Description).HasMaxLength(256);
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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.Phone).HasMaxLength(15);
            entity.Property(order => order.Address).HasMaxLength(256);
        });

        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.Property(orderTracking => orderTracking.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.Property(package => package.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.Property(request => request.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(service => service.Name).HasMaxLength(100);
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
    }
}