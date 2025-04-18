using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.CancellationReasonModels
{
    public class CancellationReasonFilterModel : FilterParameter
    {
        public Role? Role { get; set; }
        public int? Value {  get; set; } 
    }
}
