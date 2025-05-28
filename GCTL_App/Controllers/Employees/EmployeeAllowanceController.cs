using GCTL.Core.Repository;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAllowanceController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;

        public EmployeeAllowanceController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

         
            ViewBag.HouseRentAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "35", Text = "35 %" },
                new SelectListItem { Value = "40", Text = "40 %" },
                new SelectListItem { Value = "45", Text = "45 %" },
                new SelectListItem { Value = "50", Text = "50 %" },
                new SelectListItem { Value = "60", Text = "60 %" },
                new SelectListItem { Value = "70", Text = "70 %" },
                new SelectListItem { Value = "100", Text = "100 %" }
            }, "Value", "Text");

            ViewBag.MedicalAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");

            ViewBag.ConveyanceAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8", Text = "8 %" },
                new SelectListItem { Value = "10", Text = "10 %" },
                new SelectListItem { Value = "15", Text = "15 %" }
            }, "Value", "Text");

           

            SetSmartPageCode(119000);
            return View();
        }
    }
}
