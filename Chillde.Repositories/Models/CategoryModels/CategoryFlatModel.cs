namespace Chillde.Repositories.Models.CategoryModels;

public class CategoryFlatModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentAlt { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
    public Guid? ModifiedById { get; set; }
    public bool IsDeleted { get; set; }
}