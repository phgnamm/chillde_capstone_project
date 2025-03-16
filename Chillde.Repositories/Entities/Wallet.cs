namespace Chillde.Repositories.Entities;

public class Wallet : BaseEntity
{
    public decimal Balance { get; set; }

    // Relationship
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}