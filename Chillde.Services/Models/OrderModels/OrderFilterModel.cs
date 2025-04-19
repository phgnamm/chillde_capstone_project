using Chillde.Repositories.Enums;
using Chillde.Services.Common;

namespace Chillde.Services.Models.OrderModels
{
    public class OrderFilterModel : FilterParameter
    {
        public OrderStatus? Status { get; set; }
        public Guid? AccountId { get; set; }
        public Chillde.Repositories.Enums.Role? Role { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
