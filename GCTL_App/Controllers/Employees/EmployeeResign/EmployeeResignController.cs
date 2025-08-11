using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Employees.EmployeeStatus.Promotion;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeResign
{
    public class EmployeeResignController : BaseController
    {

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;

        private readonly IEmployeeResign _employeeResignService;


        public EmployeeResignController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IEmployeeResign employeeResignService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employeeResignService = employeeResignService;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111800); 

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




            return View();
        }



        [HttpGet]
        public IActionResult GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate = null, string toDate = null)
        {
            var result = _employeeResignService.GetResignations(page, pageSize, sortColumn, sortDirection, fromDate, toDate);
            return Json(result);
        }

    }
}
