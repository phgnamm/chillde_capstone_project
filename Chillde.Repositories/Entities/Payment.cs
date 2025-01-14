using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public required PaymentType PaymentType { get; set; }
        public required decimal Amount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}
