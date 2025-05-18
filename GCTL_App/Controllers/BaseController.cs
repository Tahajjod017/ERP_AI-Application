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
    }


}
