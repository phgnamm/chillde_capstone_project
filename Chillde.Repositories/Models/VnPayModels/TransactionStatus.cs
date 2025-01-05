using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.VnPayModels
{
    public class TransactionStatus
    {
        public TransactionStatusCode Code { get; set; }
        public string Description { get; set; }
    }
}
