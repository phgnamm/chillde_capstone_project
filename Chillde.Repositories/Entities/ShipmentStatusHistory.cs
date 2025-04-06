using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class ShipmentStatusHistory : BaseEntity
    {
        public Guid ShipmentId { get; set; }
        public ShipmentStatus StatusId { get; set; }
        public Shipment Shipment { get; set; } = null!;


    }
}
