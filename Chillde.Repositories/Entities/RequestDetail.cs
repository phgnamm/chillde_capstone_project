namespace Chillde.Repositories.Entities
{
    public class RequestDetail : BaseEntity
    {
        public string? Description {  get; set; }

        // Foreign key
        public Guid RequestId { get; set; }
        public Guid ItemAttributeId { get; set; }

        // Relationship
        public Request Request { get; set; } = null!;
        public ItemAttribute ItemAttribute { get; set; } = null!;
    }
}