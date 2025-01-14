using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Enums
{
    public enum BankCode : sbyte
    {
        [Description("Tất cả phương thức thanh toán")]
        ANY,
        [Description("Thanh toán quét mã QR")]
        VNPAYQR,
        [Description("Thẻ ATM hoặc tài khoản ngân hàng tại Việt Nam")]
        VNBANK,
        [Description("Thẻ thanh toán quốc tế")]
        INTCARD
    }
}
