using Chillde.Repositories.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services
{
    public class OrderAddModel
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public Decimal? TotalPrice { get; set; }
        public int? Quantity { get; set; }
        public DateTime? OrderDateTime { get; set; }
        public OrderStatus Status { get; set; }
        public Guid PaymentId { get; set; }
        public Guid PackageId { get; set; }
        public Guid ShipmentId { get; set; }
        public ICollection<OrderInformationAddModel>? OrderInformationAddModels { get; set; } = new List<OrderInformationAddModel>();
    }
    public class OrderInformationAddModel
    {
        public Guid? PackageFeatureId { get; set; }
        public string? Description { get; set; }
        public ICollection<OrderInformationAttachmentAddModel>? OrderInformationAttachmentAddModels { get; set; } = new List<OrderInformationAttachmentAddModel>();   
    }
    public class OrderInformationAttachmentAddModel
    {
       public IFormFile? AttachmentUrl { get; set; }
    }
    public class ServiceImageAddModel
    {
        public IFormFile Images { get; set; }
    }
}
