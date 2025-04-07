using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.OrderModels
{
    public class OrderFilterModel : FilterParameter
    {
        public OrderStatus? Status { get; set; }
        public Chillde.Repositories.Enums.Role? Role { get; set; }

    }
}
