namespace Chillde.Repositories.Entities
{
    public class SubCategory : BaseEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }

        // Foreign key
        public Guid CategoryId { get; set; }

        // Relationship
        public Category Category { get; set; } = null!;
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}