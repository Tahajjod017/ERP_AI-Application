using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Language;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeBenifitController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBenifitRepository;
        private readonly IEmployeeBenifitService _employeeBenifitService;
        public EmployeeBenifitController(ITranslateService translateService, IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IEmployeeBenifitService employeeBenifitService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository) : base(translateService)
        {
            _employeeBenifitRepository = employeeBenifitRepository;
            _employeeBenifitService = employeeBenifitService;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index()
        {
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");


            SetSmartPageCode(118000);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeBenifitData(int employeeId)
        {
            var employeeBenifitData = await _employeeBenifitService.GetEmployeeBenifitByEmployeeIdAsync(employeeId);
            return Ok(employeeBenifitData);

        }
    }
}