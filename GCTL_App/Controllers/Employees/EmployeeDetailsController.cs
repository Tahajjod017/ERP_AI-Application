using GCTL.Core.ViewModels;
using GCTL.Service.Employees.EmployeeDetails;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeDetailsController : BaseController
    {
        #region CTOR
        private readonly IEmployeeDetailsService _employeeDetailsService;


        public EmployeeDetailsController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeDetailsService employeeDetailsService) : base(translateService, userProfileService)
        {
            _employeeDetailsService = employeeDetailsService;
        }


        #endregion
        public IActionResult Index(int id)
        {
            ViewBag.EmployeeId = id;
            SetSmartPageCode(119000);
            return View();
        }

        public async Task<IActionResult> BasicDetail(int empID)
        {
            string imgURL = GetEmployeePictureURL();

            CommonReturnViewModel result = await _employeeDetailsService.GetBasicDetail(empID, imgURL);
            return Ok(result);
        }


    }
}
