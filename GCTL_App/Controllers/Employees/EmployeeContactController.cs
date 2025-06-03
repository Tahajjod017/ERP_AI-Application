using GCTL.Core.Repository;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeContactController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IEmployeeContactService _employeeContactService;
        public EmployeeContactController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeContactService employeeContactService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeContactService = employeeContactService;
        }

        public IActionResult Index(int id)
        {
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            SetSmartPageCode(117000);
            return View();
        }
    }
}
