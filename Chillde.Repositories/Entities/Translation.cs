namespace Chillde.Repositories.Entities
{
    public class Translation : BaseEntity
    {
        public string EntityType { get; set; } = null!;
        public Guid EntityId {  get; set; }
        public string FieldName { get; set; } = null!;
        public string TranslationText { get; set; } = null!;

        // Foreign key
        public Guid LanguageId { get; set; }

        // Relationship
        public Language Language { get; set; } = null!;
    }
}