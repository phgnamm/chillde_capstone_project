using Chillde.Repositories.Enums;
using System.Text.Json.Serialization;

namespace Chillde.Repositories.Entities;

public class Offer : BaseEntity
{
    public string? Message { get; set; }
    public OfferStatus Status { get; set; }
    public float? MinWeight { get; set; }
    public float? MaxWeight { get; set; }

    // Foreign key
    public Guid? RequestId { get; set; }
    public Guid? ServiceId { get; set; }

    // Relationship
    [JsonIgnore]
    public Account CreatedBy { get; set; } = null!;
    [JsonIgnore]
    public Package? Package { get; set; }
    [JsonIgnore]
    public Request? Request { get; set; }
    [JsonIgnore]
    public Service? Service { get; set; }
    [JsonIgnore]
    public virtual ICollection<OfferAttachment> OfferAttachments { get; set; } = new List<OfferAttachment>();
}