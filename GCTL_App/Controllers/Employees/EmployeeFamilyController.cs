
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeFamilyController : BaseController
    {

        private readonly IEmployeeFamilyService _employeeFamilyService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IEmployeeNavigationService _employeeNavigationService;


        public EmployeeFamilyController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeFamilyService employeeFamilyService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeNavigationService employeeNavigationService) : base(translateService, userProfileService)

        {
            _employeeFamilyService = employeeFamilyService;
            _employeeRepository = employeeRepository;
            _employeeNavigationService = employeeNavigationService;
        }

        public IActionResult Index(int id)
        {

            var navigationModel = _employeeNavigationService.GetEmployeeNavigation("FamilyInfo");
            ViewBag.Navigation = navigationModel;

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");


            SetSmartPageCode(116000);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(EmployeeFamilyPostViewModel model)
        {
            var res = await _employeeFamilyService.SaveAsync(model);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _employeeFamilyService.DeleteAsync(id);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeData(int id)
        {
            var employee = await _employeeFamilyService.GetEmployeeFamilyByIdAsync(id);
            return Ok(employee);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeFamilyData(int id)
        {
            try
            {
                var data = await _employeeFamilyService.GetEmployeeFamilyData(id);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] EmployeeFamilyPostViewModel model)
        {
            if (model == null || model.EmployeeFamilyInfoID <= 0)
            {
                return Ok(new { success = false, message = "Invalid data" });
            }

            var res = await _employeeFamilyService.UpdateAsync(model);
            return Ok(res);
        }

    }
}
