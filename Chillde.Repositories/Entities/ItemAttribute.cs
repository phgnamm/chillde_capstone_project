using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class ItemAttribute : BaseEntity
    {
        public string? Name {  get; set; }
        public bool IsRequired { get; set; }
        public ItemAttributeType Type { get; set; }

        // Foreign key
        public Guid ItemId { get; set; }

        // Relationship
        public Item Item { get; set; } = null!;
        public RequestDetail RequestDetail { get; set; } = null!;
    }
}