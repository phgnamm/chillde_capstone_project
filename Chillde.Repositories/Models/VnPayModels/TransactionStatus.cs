using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.VnPayModels
{
    /// <summary>
    /// Trạng thái của giao dịch sau khi được xử lý.
    /// </summary>
    public class TransactionStatus
    {
        /// <summary>
        /// Mã trạng thái của giao dịch do VNPAY định nghĩa.
        /// </summary>
        public TransactionStatusCode Code { get; set; }

        /// <summary>
        /// Mô tả chi tiết về trạng thái giao dịch.
        /// </summary>
        public string Description { get; set; }
    }
}
