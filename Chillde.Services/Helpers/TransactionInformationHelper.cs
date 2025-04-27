using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Helpers
{
    public class TransactionInformationHelper
    {
      
            public static string DepositInformation(string? orderCode, decimal? amount)
            {
            if (string.IsNullOrEmpty(orderCode))
            {
                return $"Bạn đã rút {amount} trong số dư.";
            

            }
            return $"Bạn đã nạp tiền cho đơn hàng [{orderCode}].";
            }

            public static string TransferInInformation(string orderCode, Chillde.Repositories.Enums.Role role)
            {
                return role == Chillde.Repositories.Enums.Role.Customer
                    ? $"Số dư của bạn đã được hoàn lại từ đơn hàng [{orderCode}]."
                    : $"Bạn đã nhận thanh toán vào số dư từ đơn hàng [{orderCode}].";
            }

            public static string TransferOutInformation(string orderCode)
            {
                return $"Bạn đã sử dụng số dư cho đơn hàng [{orderCode}].";
            }

    }
}
