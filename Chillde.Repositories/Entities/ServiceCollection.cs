namespace Chillde.Repositories.Entities;

public class ServiceCollection : BaseEntity
{
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }

    // Relationship
    public Account CreatedBy { get; set; } = null!;
    public virtual ICollection<ServiceWishlist> ServiceWishlists { get; set; } = new List<ServiceWishlist>();
}