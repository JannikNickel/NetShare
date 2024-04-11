using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace NetShare.Models
{
    public record TransferTarget(IPAddress Ip, string DisplayName)
    {
        public static TransferTarget? ForCurrentUser()
        {
            IPAddress? localIp = GetLocalIp();
            return localIp != null ? new TransferTarget(localIp, Environment.MachineName) : null;
        }

        public static IPAddress? GetLocalIp()
        {
            try
            {
                if(NetworkInterface.GetIsNetworkAvailable())
                {
                    IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress? localIp = host.AddressList.FirstOrDefault(n => n.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    return localIp;
                }
            }
            catch { }
            return null;
        }
    }
}
