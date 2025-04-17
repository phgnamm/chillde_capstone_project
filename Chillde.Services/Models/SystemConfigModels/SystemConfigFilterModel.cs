using Chillde.Repositories.Enums;
using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.SystemConfigModels
{
    public class SystemConfigFilterModel : FilterParameter
    {
        public ConfigType? Type { get; set; }
    }
}
