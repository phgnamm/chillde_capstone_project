using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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
        public string? Phone { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = null!;

        [Required]
        public string ToWard { get; set; } = null!;

        [Required]
        public string ToDistrict { get; set; }

        [Required]
        public string ToProvince { get; set; } = null!;

        //[Range(0, double.MaxValue, ErrorMessage = "Total price must be non-negative.")]
        //public decimal? TotalPrice { get; set; }

        public decimal? ShippingPrice { get; set; }

        //[Required]
        //[Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int? Quantity { get; set; } = 1;

        //public string? ShipmentCode { get; set; }


        //public OrderStage Stage { get; set; } = OrderStage.ReviewRequirement;

        //public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public Guid PackageId { get; set; }

        public bool? WithBalance { get; set; } = false;

        public ICollection<OrderInformationAddModel>? OrderInformationAddModels { get; set; } = new List<OrderInformationAddModel>();
        public ICollection<Guid>? VoucherId { get; set; }
    }

    public class OrderInformationAddModel
    {
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        //public decimal? Price { get; set; }

        [Required]
        public Guid PackageFeatureId { get; set; }
        public ICollection<OrderInformationAttachmentAddModel>? OrderInformationAttachmentAddModels { get; set; } = new List<OrderInformationAttachmentAddModel>();

    }
    public class OrderInformationAttachmentAddModel
    {
      public string? AttachmentAlt { get; set; }
      public IFormFile? AttachmentUrl { get; set; }
    }


}
