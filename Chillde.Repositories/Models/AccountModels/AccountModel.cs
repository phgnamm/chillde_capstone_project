using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.ShippingAddressModels;
using Role = Chillde.Repositories.Enums.Role;

namespace Chillde.Repositories.Models.AccountModels;

public class AccountModel : BaseEntity
{
    // Required information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Personal information
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? StoreAddress { get; set; }
    public string? StoreDescription { get; set; }
    public string? Image { get; set; }
    public string? Banner { get; set; }
    public double? SuccessDeliveryRate { get; set; }

    // Status
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    //public AccountStatus Status { get; set; }
    // Wallet information
    public decimal Balance { get; set; } // Lấy số dư ví

    // AccountRole information
    public List<int> TotalReputations { get; set; } = null!;
    public List<AccountStatus> Status { get; set; } = null!;
    public List<string> StatusNames { get; set; } = null!;
    
    // Order information
    public int OrderCount { get; set; }
    // Relationship
    public List<Role> Roles { get; set; } = null!;
    public List<string> RoleNames { get; set; } = null!;
    public List<string> Skills { get; set; } = null!;
    public List<ShippingAddressModel>? ShippingAddresses { get; set; }
}