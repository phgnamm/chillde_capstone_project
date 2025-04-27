using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentDetailModel : BaseEntity
    {
        public string? TrackingId { get; set; }
        public ShipmentStatus CurrentStatusId { get; set; }
        public string? PartnerId { get; set; }
        public string? EstimatedPickTime { get; set; }
        public string? EstimatedDeliverTime { get; set; }
        public List<ProductShipmentResponse> ProductShipments { get; set; } = new List<ProductShipmentResponse>();
    }

    public class ProductShipmentResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}

