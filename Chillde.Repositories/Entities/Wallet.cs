namespace Chillde.Repositories.Entities
{
    public class Wallet : BaseEntity
    {
        public Decimal? Balance { get; set; }

        // Relationship
        public Account CreatedBy { get; set; } = null!;
        public virtual ICollection<WalletHistory> WalletHistories { get; set;} = new List<WalletHistory>();
    }
}