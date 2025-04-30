using Chillde.Repositories.Enums;
using System.Text.Json;

namespace Chillde.Repositories.Entities
{
    public class SystemConfig : BaseEntity
    {
        public ConfigType EntityType { get; set; }
        public string? FieldName { get; set; }
        public DateOnly? EffectiveFrom { get; set; }
        public bool IsActive { get; set; }
        public JsonDocument Value { get; set; } = null!;
    }
}
