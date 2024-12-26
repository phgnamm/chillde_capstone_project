using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ShippingAddressModels
{
    public class DistrictModel
    {
        public int DistrictID { get; set; }
        public int ProvinceID { get; set; }
        public string? DistrictName { get; set; }
        public int? Code { get; set; }
    }
}
