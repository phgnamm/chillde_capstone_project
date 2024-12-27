using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.OrderModels
{
    public class OrderAddModel
    {
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required decimal TotalPrice { get; set; }
        public required int Quantity { get; set; }
        public DateTime? OrderDateTime { get; set; }
        public OrderStatus Status { get; set; }
        public Guid PaymentId { get; set; }
        public Guid PackageId { get; set; }
        public Guid ShipmentId { get; set; }
    }
}
