using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees.EmployeeStatusManagement.IncrementManagement
{
    public class IncrementListController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        public IncrementListController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
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

        #region Get ALL For Table

        [HttpPost]
        public IActionResult GetIncrementList(string searchTerm, int? departmentId, string incrementType, string dateRange, int pageSize = 10, int pageNumber = 1)
        {
           // var result = _yourService.GetFilteredIncrementList(searchTerm, departmentId, incrementType, dateRange, pageSize, pageNumber);

            return Json(new
            {
                success = true,
               // data = result
            });
        }


        #endregion
    }
}
