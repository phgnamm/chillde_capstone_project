namespace Chillde.Repositories.Entities;

public class Category : BaseEntity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? AttachmentAlt { get; set; }
    public string? AttachmentUrl { get; set; }

    // Foreign key
    public Guid? ParentId { get; set; }
    
    // Relationship
    public Category? Parent { get; set; }
}