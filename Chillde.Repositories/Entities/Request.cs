using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Request : BaseEntity
    {
        public string? Name {  get; set; }
        public string? Description {  get; set; }
        public Decimal? MinBudget { get; set; }
        public Decimal? MaxBudget { get; set; }
        public int? Timeline {  get; set; }
        public string? AttachmentUrl {  get; set; }
        public RequestStatus Status { get; set; }

        // Foreign key
        public Guid ItemId { get; set; }

        // Relationship
        public Account CreatedBy { get; set; } = null!;
        public Item Item { get; set; } = null!;
        public virtual ICollection<RequestDetail> RequestDetails { get; set; } = new List<RequestDetail>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}