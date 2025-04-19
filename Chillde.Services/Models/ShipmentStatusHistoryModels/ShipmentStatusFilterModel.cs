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
        protected override int MinPageSize { get; set; } = 10;

    }
}
