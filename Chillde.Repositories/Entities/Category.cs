namespace Chillde.Repositories.Entities
{
    public class Category : BaseEntity
    {
        public string? Code {  get; set; }
        public string? Name {  get; set; }
        public string? ImageUrl { get; set; }

        // Relationship
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}