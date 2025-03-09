using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Entities
{
    public class VoucherUsageLog : BaseEntity
    {

        public Guid VoucherId { get; set; } // đã sử dụng voucher nào
        public Voucher Voucher { get; set; }
        public Guid CustomerId { get; set; } // khách hàng nào sử dụng
        public Account Customer { get; set; }
        public Guid OrderId { get; set; } // sử dụng cho order nào
        public Order Order { get; set; }
        public decimal DiscountValue { get; set; } // giá giảm quy ra tiền 
        public decimal DiscountValueOrigin { get; set; } // phần trăm giảm ban đầu

    }
}
