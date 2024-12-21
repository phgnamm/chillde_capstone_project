using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Language : BaseEntity
{
    public LanguageCode Code { get; set; }
    public string? Name { get; set; }

    // Relationship
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
}