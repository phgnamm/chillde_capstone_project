using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.DashBoardModels
{
    public class ArtisanDashboardModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrder { get; set; }
        public int TotalOrderPerDay { get; set; }
        public int TotalPendingOrder { get; set; }
        public int TotalDeliveredOrder { get; set; }
        public int TotalCancelOrder { get; set; }
        public int TotalActiveOrder { get; set; }
        public List<Earning> Earnings { get; set; }

    }
    public class Earning
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
    }

}
