using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

namespace Chillde.Services.Helpers
{
    public class NetworkHelper
    {

        //public static string GetIpAddress(HttpContext context)
        //{
        //    var remoteIpAddress = context.Connection.RemoteIpAddress;

        //    if (remoteIpAddress != null)
        //    {
        //        var ipv4Address = Dns.GetHostEntry(remoteIpAddress).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

        //        return remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipv4Address != null
        //            ? ipv4Address.ToString()
        //            : remoteIpAddress.ToString();
        //    }

        //    throw new InvalidOperationException("Không tìm thấy địa chỉ IP");
        //}
        public static string GetIpAddress(HttpContext context)
        {
            // Nếu có dùng proxy/load balancer (Azure, Nginx...), ưu tiên lấy IP từ header
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // Trường hợp có nhiều IP thì lấy IP đầu tiên
                var ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (IPAddress.TryParse(ip, out var parsedIp))
                {
                    return parsedIp.ToString();
                }
            }

            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp != null)
            {
                // Nếu là IPv6 thì map về IPv4 nếu có
                if (remoteIp.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIp = remoteIp.MapToIPv4();
                }

                return remoteIp.ToString();
            }

            throw new InvalidOperationException("Không tìm thấy địa chỉ IP");
        }
    }
}
