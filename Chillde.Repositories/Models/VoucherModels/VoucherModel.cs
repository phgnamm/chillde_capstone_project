using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Models.VoucherModels
{
    public class VoucherModel : BaseEntity
    {
        public required string Code { get; set; }
        public  string ReceiverName { get; set; }
        public Guid? ReceiverId { get; set; }
        public decimal DiscountValue { get; set; } 
        public decimal? MinOrderValue { get; set; } 
        public decimal? MaxDiscountValue { get; set; }
        public int? RemainingQuantity { get; set; } 
        public int? TotalQuantity { get; set; }
        public DateTime StartTime { get; set; } 
        public DateTime ExpiredTime { get; set; }
        public VoucherStatus VoucherStatus { get; set; }
        public List<VoucherUsageLog>? VoucherUsageLogs { get; set; } = new List<VoucherUsageLog>();
    }
}
