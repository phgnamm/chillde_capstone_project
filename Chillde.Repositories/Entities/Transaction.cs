using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Transaction : BaseEntity
{
    public decimal? Amount { get; set; }
    public TransactionType Type { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    // Foreign key
    public Guid WalletId { get; set; }
    public Guid OrderId { get; set; }

    // Relationship
    public Wallet Wallet { get; set; } = null!;
    public Order Order { get; set; } = null!;
}