namespace Chillde.Repositories.Entities
{
    public class Item : BaseEntity
    {
        public string? Code { get; set; }
        public String? Name { get; set; }
        public String? ImageUrl { get; set; }

        // Foreign key
        public Guid SubCategoryId { get; set; }

        // Relationship
        public SubCategory SubCategory { get; set; } = null!;
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<ItemAttribute> ItemAttributes { get; set; } = new List<ItemAttribute>();
    }
}