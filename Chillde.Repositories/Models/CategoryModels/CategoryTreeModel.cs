using Chillde.Repositories.Entities;
using Microsoft.AspNetCore.Http;

namespace Chillde.Repositories.Models.CategoryModels;

public class CategoryTreeModel : BaseEntity
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentAlt { get; set; }
    public List<CategoryTreeModel>? Children { get; set; } 
}