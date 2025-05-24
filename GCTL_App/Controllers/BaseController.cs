using System.Net.NetworkInformation;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers
{
    public abstract class BaseController : Controller
    {



        //protected readonly ITranslateService _translateService;

        //private int _smartPageCode = 0;

        //protected BaseController()
        //{
        //}

        //protected BaseController(ITranslateService translateService)
        //{
        //    _translateService = translateService;
        //}

        //protected void SetSmartPageCode(int code)
        //{
        //    _smartPageCode = code;
        //    ViewData["SmartPageCode"] = _smartPageCode;
        //    ViewData["BaseControllerInstance"] = this;
        //}

        //protected string SmartLocalizeText(string defaultText)
        //{
        //    string lang = HttpContext.Items["Language"] as string ?? "en";
        //    return _translateService.GetTranslationInd(defaultText, (_smartPageCode++).ToString(), lang) ?? defaultText;
        //}


        protected readonly ITranslateService _translateService;


        private int _smartPageCode = 0;

        protected BaseController(ITranslateService translateService)
        {
            _translateService = translateService;
        }

        protected void SetSmartPageCode(int code)
        {
            _smartPageCode = code;
            ViewData["SmartPageCode"] = _smartPageCode;
            ViewData["BaseControllerInstance"] = this;
        }

        protected string SmartLocalizeText(string defaultText)
        {
            string lang = HttpContext.Items["Language"] as string ?? "en";
            return _translateService.GetTranslationInd(defaultText, (_smartPageCode++).ToString(), lang);
        }






        //private int _pageCode;
        //protected string LanguageCode => HttpContext.Items["Language"] as string ?? "en";


        // Call this in each controller to set the correct pageCode
        //protected void SetPageCode(int startCode)
        //{
        //    _pageCode = startCode;
        //}

        //protected string Translate(string defaultText)
        //{
        //    return _translateService.GetTranslationInd(defaultText, (_pageCode++).ToString(), LanguageCode);
        //}

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
