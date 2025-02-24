namespace Chillde.Repositories.Entities;

public class OrderInformation : BaseEntity
{
    public string? Description {  get; set; }

    #region new fields
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    #endregion

    // Foreign key
    public Guid PackageFeatureId { get; set; }
    public Guid OrderId { get; set; }

    // Relationship
    public PackageFeature PackageFeature { get; set; } = null!;
    public Order Order { get; set; } = null!;

    public virtual ICollection<OrderInformationAttachment> OrderInformationAttachments { get; set; } =
        new List<OrderInformationAttachment>();
}