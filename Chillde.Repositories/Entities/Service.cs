using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Service : BaseEntity
    {
        public string? Name {  get; set; }
        public string? Description { get; set; }
        public bool IsOffter {  get; set; }
        public ServiceStatus Status { get; set; }

        // Foreign key
        public Guid AccountId { get; set; }
        public Guid ItemId { get; set; }

        // Relationship
        public Account Account { get; set; } = null!;
        public Item Item { get; set; } = null!;
        public virtual ICollection<FAQ> FAQs { get; set; } = new List<FAQ>();
        public virtual ICollection<ServiceWishlist> ServiceWishlists { get; set; } = new List<ServiceWishlist>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<ServiceImage> ServiceImages { get; set; } = new List<ServiceImage>();
        public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
    }
}