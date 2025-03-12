using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Transaction : BaseEntity
{
    public decimal? Amount { get; set; }
    public WalletHistoryType Type { get; set; }

    public WalletHistoryStatus Status { get; set; }

    // Foreign key
    public Guid WalletId { get; set; }

    // Relationship
    public Wallet Wallet { get; set; } = null!;
}