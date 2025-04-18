using Chillde.Services.Common;

namespace Chillde.Services.Models.DepositModels
{
    public class DepositFilterModel : FilterParameter
    {
        public Guid? AccountId { get; set; }
    }
}
