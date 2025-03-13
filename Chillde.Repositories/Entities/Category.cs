using Microsoft.IdentityModel.Logging;
using Slugify;

namespace Chillde.Repositories.Entities;

public class Category : BaseEntity
{
    private static readonly SlugHelper _slugHelper = new();
    private string? _name;
    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            Slug = value != null ? _slugHelper.GenerateSlug(value) : null;
        }
    }

    public string? Slug { get; private set; }
    public string? AttachmentAlt { get; set; }
    public string? AttachmentUrl { get; set; }

    // Foreign key
    public Guid? ParentId { get; set; }
    
    // Relationship
    public Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();
}