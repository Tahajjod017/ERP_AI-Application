using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.LipLmacAddress
{
    public static class NetworkHelper
    {
        public static string GetLocalIP()
        {
            try
            {
                string ipAddress = string.Empty;
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var properties = networkInterface.GetIPProperties();
                    var ipv4Address = properties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        ipAddress = ipv4Address.Address.ToString();
                        break;
                    }
                }
                
                return ipAddress;
            } catch(Exception ex)
            {

                Console.WriteLine(ex.Message);
                return "0.0.0.0";
            }
          
        }
        public static string GetLocalIPMobile(HttpContext httpContext)
        {
            try
            {
                string ipAddress = string.Empty;
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var properties = networkInterface.GetIPProperties();
                    var ipv4Address = properties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        ipAddress = ipv4Address.Address.ToString();
                        break;
                    }
                }

                // fallback: client IP from HttpContext
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "0.0.0.0")
                {
                    var clientIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                    ipAddress = clientIp ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                }

                return ipAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return httpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            }
        }
        public static string GetMacAddress()
        {
            try
            {
                string macAddress = string.Empty;
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    macAddress = networkInterface.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        break;
                    }
                }
               return string.IsNullOrEmpty(macAddress) ? "Unavailable" : macAddress;
                //return macAddress ?? "From Mobile";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "0.0.0.0";
            }
            
        }

    }
}
