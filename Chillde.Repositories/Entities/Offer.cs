using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Offer : BaseEntity
    {
        public string? Message {  get; set; }
        public OfferStatus Status { get; set; }

        // Foreign key
        public Guid AccountId { get; set; }
        public Guid RequestId { get; set; }
        public Guid ServiceId { get; set; }

        // Relationship
        public Account Account { get; set; } = null!;
        public Request Request { get; set; } = null!;
        public Service Service { get; set; } = null!;
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}