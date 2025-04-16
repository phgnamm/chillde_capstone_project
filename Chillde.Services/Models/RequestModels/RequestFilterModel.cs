using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.RequestModels
{
    public class RequestFilterModel : FilterParameter
    {
        public bool ViewAll { get; set; }
    }
}
