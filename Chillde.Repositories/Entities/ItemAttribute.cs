using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class ItemAttribute : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid AttributeId { get; set; }

    // Relationship
    public Item Item { get; set; } = null!;
    public Attribute Attribute { get; set; } = null!;
}