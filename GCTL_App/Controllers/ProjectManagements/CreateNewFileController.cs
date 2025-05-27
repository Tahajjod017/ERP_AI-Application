using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.ProjectManagements
{
    public class CreateNewFileController : BaseController
    {
        public CreateNewFileController(ITranslateService translateService) : base(translateService)
        {
        }

        public IActionResult Index()
        {
            SetSmartPageCode(3002000);
            return View();
        }
    }
}
