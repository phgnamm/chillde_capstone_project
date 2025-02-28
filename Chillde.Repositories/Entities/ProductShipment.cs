using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class ProductShipment : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Tên sản phẩm
        public decimal Weight { get; set; } // Khối lượng
        public int Quantity { get; set; } // Số lượng
        public string ProductCode { get; set; } = string.Empty; // Mã sản phẩm
        public Guid ShipmentId { get; set; } // FK đến Shipment

        // Relationship
        public Shipment Shipment { get; set; } = null!;
    }
}
