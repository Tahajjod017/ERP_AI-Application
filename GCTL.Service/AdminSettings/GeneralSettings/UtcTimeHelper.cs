using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public static class UtcTimeHelper
    {
        private const string NtpServer = "time.google.com";

        public static DateTime GetReliableUtcNow()
        {
            try
            {
                return GetNetworkUtcTime();
            }
            catch
            {
                // If NTP fails, fallback to system clock
                return DateTime.UtcNow;
            }
        }

        private static DateTime GetNetworkUtcTime()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // NTP request settings

            var addresses = System.Net.Dns.GetHostEntry(NtpServer).AddressList;
            var ipEndPoint = new System.Net.IPEndPoint(addresses[0], 123);

            using var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = 3000 // 3 seconds timeout
            };

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);

            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData.Skip(serverReplyTime).Take(4).Reverse().ToArray(), 0);
            ulong fractPart = BitConverter.ToUInt32(ntpData.Skip(serverReplyTime + 4).Take(4).Reverse().ToArray(), 0);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToUniversalTime();
        }
    }

}
