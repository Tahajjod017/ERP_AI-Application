using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeStatus.Increment;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;

        private readonly IincrementService _incrementService;



        public IncrementController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository, IincrementService incrementService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
            _incrementService = incrementService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111700);
            

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            ViewBag.OrganizationDD = new SelectList(
               _organizationRepository.All().Select(o => new { o.OrganizationID, o.OrganizationName }),
               "OrganizationID",
               "OrganizationName"
           );

           

            ViewBag.DepartmentDD = new SelectList(
                _departmentRepository.All().Select(d => new { d.DepartmentID, d.DepartmentName }),
                "DepartmentID",
                "DepartmentName"
            );

            ViewBag.DesignationDD = new SelectList(
                _designationRepository.All().Select(d => new { d.DesignationID, d.DesignationName }),
                "DesignationID",
                "DesignationName"
            );

            return View();
        }


        #region Save 

        [HttpPost]
        public async Task< IActionResult> SaveSalaryChange(SalaryChangeViewModel model)
        {
            CommonReturnViewModel result = await _incrementService.SaveAsync(model);
            return Ok(result);
        }


        #endregion

        #region On change Of Emp

        [HttpGet]
        public IActionResult GetEmployeeDetails(int employeeId)
        {
            // Fetch employee info from your existing service
            var emp = _employeeOfficialRepository.FirstOrDefaultAsync(e => e.EmployeeID == employeeId).Result;
            var sal = _employeeSalaryRepository.FirstOrDefaultAsync(e => e.EmployeeID == employeeId).Result;
            if (emp == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new
                {
                    organizationId = emp.OrganizationID,
                    designationId = emp.DesignationID,
                    departmentId = emp.DepartmentID,
                    currentSalary = sal.Salary
                }
            });
        }


        #endregion
    }
}
