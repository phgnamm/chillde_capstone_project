using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.PaymentModels
{
    public class PaymentInformationAddModel
    {
        public string OrderID { get; set; } = null!;
        public string AccountID { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public required double Amount { get; set; }
    }
}
