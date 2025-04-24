using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.DashBoardModels
{
    public class AdminDashboardModel
    {
        public RevenueStat Revenue { get; set; } = default!;
        public UserStat Users { get; set; } = default!;
        public OrderStat Orders { get; set; } = default!;
        public List<RevenueChart> RevenueCharts { get; set; } = default!;
    }
    public class RevenueStat
    {
        public decimal Total { get; set; }
        public double ChangePercentage { get; set; }
        public string ComparedTo { get; set; } = "lastMonth";
    }

    public class UserStat
    {
        public int Total { get; set; }
        public double ChangePercentage { get; set; }
        public string ComparedTo { get; set; } = "lastMonth";
        public UserDetail Details { get; set; } = default!;
    }

    public class UserDetail
    {
        public int Customer { get; set; }
        public int Artisan { get; set; }
    }

    public class OrderStat
    {
        public int Total { get; set; }
        public double ChangePercentage { get; set; }
        public int TotalCompleteOrder { get; set; }
        public int TotalCancelOrders { get; set; }
        public string ComparedTo { get; set; } = "yesterday";
    }
    public class RevenueChart
    {
        public decimal TotalPriceWithoutShipFee { get; set; }
        public decimal TotalArtisanRevenue { get; set; }
        public decimal TotalPlatformFee { get; set; }
        public DateOnly Month {  get; set; }
    }
}
