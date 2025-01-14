using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.VnPayModels
{
    public class BankingInfor
    {
        /// <summary>
        /// Mã ngân hàng thực hiện giao dịch. ví dụ: VCB (Vietcombank), BIDV (Ngân hàng Đầu tư và Phát triển Việt Nam).
        /// </summary>
        public string BankCode { get; set; }

        /// <summary>
        /// Mã giao dịch ở phía ngân hàng, được dùng để theo dõi và đối soát giao dịch với ngân hàng.
        /// </summary>
        public string BankTransactionId { get; set; }
    }
}
