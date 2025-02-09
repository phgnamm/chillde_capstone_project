using System.Text.Json;

namespace Chillde.Repositories.Entities
{
    public class SystemConfig : BaseEntity
    {
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string FieldName { get; set; } = null!;
        public JsonDocument Value { get; set; } = null!;
    }
}
