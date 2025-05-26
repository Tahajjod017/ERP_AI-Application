using System.Net.NetworkInformation;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers
{
    public abstract class BaseController : Controller
    {


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

    }


}
