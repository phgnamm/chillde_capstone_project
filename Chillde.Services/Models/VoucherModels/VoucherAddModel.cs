using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Models.VoucherModels
{
    public class VoucherAddModel
    {
        public Guid? ReceiverId { get; set; }
        public int? MinOrderRequired { get; set; }
        public int? MinReputation { get; set; }

        [Required(ErrorMessage = "Discount value is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0.")]
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderValue { get; set; } 
        public decimal? MaxDiscountValue { get; set; } 
        public int? TotalQuantity { get; set; } 
        public int? RemainingQuantity { get; set; }
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date format.")]
        public DateTime ExpiredTime { get; set; } 
    }
}
