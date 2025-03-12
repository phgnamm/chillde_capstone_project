using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class Voucher : BaseEntity
    {
        public Guid? ReceiverId { get; set; } // ID của đối tượng nhận voucher cụ thể là nghệ nhân nếu admin tạo voucher (có thể null)
        public Account Creator { get; set; } = null!; // Người tạo voucher (Admin hoặc Nghệ nhân)
        public Account? Receiver { get; set; } // Người nhận voucher là nghệ nhân, không cần phải khách hàng cụ thể nếu như khách hàng đó quá vip với nghệ nhân đó
        public VoucherType VoucherType { get; set; } = VoucherType.ArtistToCustomer;
        public required string Code { get; set; }
        public int? MinOrderRequired { get; set; } // số lượng đơn mà khách hàng đã hoàn thành với nghệ nhân đó
        public int? MinReputation { get; set; } // số uy tín tối thiểu mà nghệ nhân muốn voucher này thuộc về khách hàng nào
        public decimal DiscountValue { get; set; } // phần trăm giảm
        public decimal? MinOrderValue { get; set; } // số đơn tối thiểu
        public decimal? MaxDiscountValue { get; set; } // giảm tối đa
        public int? TotalQuantity { get; set; } // số lượng voucher
        public int? RemainingQuantity { get; set; } // số lượng voucher còn lại
        public DateTime ExpiredTime { get; set; } // thời gian hết hạn
        public VoucherStatus VoucherStatus { get; set; } = VoucherStatus.Pending;
        public virtual ICollection<VoucherUsageLog> VoucherUsageLogs { get; set; } = new List<VoucherUsageLog>();

    }
}
