using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class AccountRole : BaseEntity
{
    public int TotalReputation { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.PendingVerification;

    // Foreign key
    public Guid AccountId { get; set; }
    public Guid RoleId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public virtual ICollection<ReputationLog> Reputations { get; set; } = new List<ReputationLog>();
}