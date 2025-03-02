using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Account : BaseEntity
{
    // Required information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashedPassword { get; set; } = null!;

    // Personal information
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Image { get; set; }

    // Artisan information
    public string? StoreAddress { get; set; }
    public string? Banner { get; set; }
    public double? SuccessDeliveryRate { get; set; }
    public string? StoreDescription { get; set; }

    // Status
    public bool EmailConfirmed { get; set; } = false;
    public bool PhoneNumberConfirmed { get; set; } = false;
    public AccountStatus Status { get; set; } = AccountStatus.PendingVerification;

    // Reputation Point 
    public int ReputationPoints { get; set; } = 12;

    // System
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiryTime { get; set; }
    public string? ResetPasswordToken { get; set; }

    // Foreign key
    public Guid WalletId { get; set; }

    // Relationship
    public Wallet Wallet { get; set; } = null!;

    public virtual ICollection<AccountConversation> AccountConversations { get; set; } =
        new List<AccountConversation>();

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<ServiceCollection> ServiceCollections { get; set; } = new List<ServiceCollection>();
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
    public virtual ICollection<Message> Message { get; set; } = new List<Message>();
    public virtual ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    public virtual ICollection<ReputationLog> Reputations { get; set; } = new List<ReputationLog>();

}