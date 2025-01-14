using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OrderModels
{
    public class OrderAddModel
    {
        [Required]
        [Phone]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string Phone { get; set; } = null!;

        [Required]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = null!;

        //[Required]
        //[Range(1, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        //public decimal TotalPrice { get; set; }

        //[Range(1, double.MaxValue, ErrorMessage = "Package price must be greater than 0.")]
        //public decimal? PackagePrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; } = 1;

        //public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public Guid PackageId { get; set; }
        public bool? WithBalance { get; set; } = false;

        [Required]
        [MinLength(1, ErrorMessage = "The payment list must contain at least one item.")]
        //public ICollection<PaymentAddModel> PaymentAddModels { get; set; } = null!;

        public ICollection<OrderInformationAddModel>? OrderInformationAddModels { get; set; }
    }

    //public class PaymentAddModel
    //{
    //    //[Required]
    //    //public PaymentType PaymentType { get; set; } = PaymentType.VnPay;

    //    [Required]
    //    [Range(1, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0.")]
    //    public decimal Amount { get; set; }

    //    //[Required]
    //    //public PaymentStatus PaymentStatus { get; set; }
    //}

    public class OrderInformationAddModel
    {
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required]
        public Guid PackageFeatureId { get; set; }
    }


}
