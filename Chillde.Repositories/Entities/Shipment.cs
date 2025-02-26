using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Shipment : BaseEntity
{
    public string? Code { get; set; }
    public int? FromDistrictId { get; set; }
    public int? FromWardCode { get; set; }
    public int? ToDistrictId { get; set; }
    public int? ToWardCode { get; set; }
    public decimal? ShippingFee { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public int? Length { get; set; }
    public decimal? Weight { get; set; }    
    public ShipmentStatus Status { get; set; }

    // Foreign key
    public Guid OrderId { get; set; }

    // Relationship
    public Order Order { get; set; } = null!;
}