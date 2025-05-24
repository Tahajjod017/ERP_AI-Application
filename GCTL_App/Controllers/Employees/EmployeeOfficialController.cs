using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeOfficialController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        public EmployeeOfficialController(ITranslateService translateService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository) : base(translateService)
        {
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index(int id)
        {
            id = 11;

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All(), "EmployeeID", "FirstName");

            var empPersonal = _employeeRepository.AllActive().FirstOrDefault(e => e.EmployeeID == id);

            EmployeeOfficialPostViewModel model = new EmployeeOfficialPostViewModel()
            {
                EmployeePersonalId = empPersonal.EmployeeID,
                PersonalEmail = empPersonal.Email,
                PersonalPhone= empPersonal.MobileNumber
            };

            SetSmartPageCode(112000);
            return View(model);
        }
    }
}
