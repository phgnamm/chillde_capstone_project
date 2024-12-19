namespace Chillde.Repositories.Entities
{
    public class Feedback : BaseEntity
    {
        public int? Rating { get; set; }
        public string? Description { get; set; }

        // Foreign key
        public Guid ServiceId { get; set; }

        // Relationship
        public Service Service { get; set; } = null!;
        public virtual ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();
    }
}