namespace Chillde.Repositories.Entities
{
    public class Wallet : BaseEntity
    {
        public Decimal? Balance { get; set; }

        // Foreign key
        public Guid AccountId { get; set; }

        // Relationship
        public Account Account { get; set; } = null!;
        public virtual ICollection<WalletHistory> WalletHistories { get; set;} = new List<WalletHistory>();
    }
}