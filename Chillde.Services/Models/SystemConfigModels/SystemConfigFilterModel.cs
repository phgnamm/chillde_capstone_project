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
        //public bool? IsActive { get; set; }
        public  SortOptions OrderOption { get; set; } = SortOptions.CreationDate;
        protected override int MinPageSize { get; set; } = 1000;
        protected override int MaxPageSize { get; set; } = 1000;

    }
}
