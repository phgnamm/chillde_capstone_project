using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.VnPayModels
{
    public class PaymentResult
    {
        public Guid OrderId { get; set; }      
        public bool IsSuccess { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public long VnpayTransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentResponse PaymentResponse { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public BankingInfor BankingInfor { get; set; }
    }
}
