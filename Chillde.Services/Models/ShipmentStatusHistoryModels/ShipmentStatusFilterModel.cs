using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ShipmentStatusHistoryModels
{
    public class ShipmentStatusFilterModel: FilterParameter
    {
        public required string PartnerId { get; set; }
        public required string Label { get; set; }
    }
}
