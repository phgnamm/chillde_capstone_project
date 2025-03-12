using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherAddModel
    {
        public Guid? ReceiverId { get; set; }
        public VoucherType VoucherType { get; set; } = VoucherType.ArtistToCustomer;
        public required string Code { get; set; }
        public int? MinOrderRequired { get; set; }
        public int? MinReputation { get; set; } 
        public decimal DiscountValue { get; set; } 
        public decimal? MinOrderValue { get; set; } 
        public decimal? MaxDiscountValue { get; set; } 
        public int? TotalQuantity { get; set; } 
        public int? RemainingQuantity { get; set; } 
        public DateTime ExpiredTime { get; set; } 
        public VoucherStatus VoucherStatus { get; set; }
    }
}
