using GCTL.Core.ViewModels;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees.EmployeeTransferManagements
{
    public class EmpTransferApprovarController : BaseController
    {
        public EmpTransferApprovarController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
