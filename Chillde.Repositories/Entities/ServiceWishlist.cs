namespace Chillde.Repositories.Entities
{
    public class ServiceWishlist : BaseEntity
    {
        // Foreign key
        public Guid ServiceId { get; set; }
        public Guid ServiceCollectionId { get; set; }

        // Relationship
        public Service Service { get; set; } = null!;
        public ServiceCollection ServiceCollection { get; set; } = null!;
    }
}