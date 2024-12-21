namespace Chillde.Repositories.Entities;

public class Skill : BaseEntity
{
    // Foreign key
    public Guid AccountId { get; set; }
    public Guid SubCategoryId { get; set; }

    // Relationship
    public Account Account { get; set; } = null!;
    public SubCategory SubCategory { get; set; } = null!;
}