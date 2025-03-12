using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities;

public class Order : BaseEntity
{
    public required string Code { get; set; }
    public string? Phone { get; set; }
    public string Address { get; set; }
    public string ToWard { get; set; }
    public int ToDistrict { get; set; }
    public string ToProvince { get; set; }
    public decimal? TotalPrice { get; set; }
    public decimal? PackagePrice { get; set; } // cân nhắc bỏ
    public int? Quantity { get; set; }
    public string? ShipmentCode { get; set; }
    
    //ly do huy
    public CancleOrderReason CancleOrderReason { get; set; }

    #region new fields
    public DateTime? DeliveryTime { get; set; }
    public int? CurrentSketchRevision { get; set; }
    public OrderStage Stage { get; set; } = OrderStage.ReviewRequirement;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    #endregion
    // Voucher
    public decimal? TotalValue { get; set; }
    public decimal? FinalValue { get; set; }
    public decimal? AdminCommission { get; set; } // check neu nghe nhan co voucher trong khoang thoi gian nay thi su dung
    public decimal? ArtistRevenue { get; set; } // doanh thu cua nghe nhan sau khi tru hoa hong cua admin
    public decimal? VoucherCost { get; set; } // tong voucher ma khach hang apply vao    

    // Foreign key
    public Guid PackageId { get; set; }

    // Relationship
    public Package Package { get; set; } = null!;
    public Account CreatedBy { get; set; } = null!;
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public virtual ICollection<VoucherUsageLog> VoucherUsageLogs { get; set; } = new List<VoucherUsageLog>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderInformation> OrderInformations { get; set; } = new List<OrderInformation>();
    public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}