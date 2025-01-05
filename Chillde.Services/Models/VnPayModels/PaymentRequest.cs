using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.VnPayModels
{

    public class PaymentRequest
    {    
        public required Guid OrderId { get; set; }
        public required string Description { get; set; }
        public required double Money { get; set; }
        public required string IpAddress { get; set; }
        public BankCode BankCode { get; set; } = BankCode.ANY;
        public DateTime CreatedDate { get; set; } = DateTime.Now;    
        public Currency Currency { get; set; } = Currency.VND;     
       public DisplayLanguage Language { get; set; } = DisplayLanguage.Vietnamese;
    }
}
