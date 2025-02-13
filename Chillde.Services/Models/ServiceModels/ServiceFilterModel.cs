using Chillde.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceFilterModel : FilterParameter
    {
        //public string? Description { get; set; } = string.Empty;
        public bool IsSuggestion { get; set; } = false;
        public bool IsAccountSuggestion { get; set; } = false;
        public bool IsEvent { get; set; } = false;
    }
}
