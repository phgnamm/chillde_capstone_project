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
        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }
    }
}
