using System.Net.NetworkInformation;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ITranslateService _translateService;

        protected BaseController(ITranslateService translateService)
        {
            _translateService = translateService;
        }


        //Added LIP LMacAddress
        public string GetLocalIP()
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
        }


        public string GetMacAddress()
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

            return macAddress;
        }
    }


}
