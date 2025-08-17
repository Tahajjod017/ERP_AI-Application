using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeTermination;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeTermination;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.Employees.EmployeeTermination
{
    public class EmployeeTerminationController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<TerminationTypes> _terminationTypeRepository;
        private readonly IEmployeeTermination _employeeTerminationService;

        public EmployeeTerminationController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository,
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Designations> designationRepository,
            IGenericRepository<TerminationTypes> terminationTypeRepository,
            IEmployeeTermination employeeTerminationService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _terminationTypeRepository = terminationTypeRepository;
            _employeeTerminationService = employeeTerminationService;
        }

        #endregion

        #region Index N Table load

        public IActionResult Index()
        {
            SetSmartPageCode(11165900);

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.TerminationTypeDD = new SelectList(_terminationTypeRepository.AllActive(), "TerminationTypeID", "TerminationTypeName");

            return View();
        }

        [HttpGet]
        public IActionResult GetTerminations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate = null, string toDate = null)
        {
            var result = _employeeTerminationService.GetTerminations(page, pageSize, sortColumn, sortDirection, fromDate, toDate, imgSrcThumb);
            return Json(result);
        }

        #endregion

        #region Create 

        [HttpPost]
        public async Task<IActionResult> CreateTermination([FromForm] TerminationPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var result = await _employeeTerminationService.InsertTermination(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        #endregion

        #region Edit and Get By ID

        [HttpGet]
        public IActionResult GetTermination(int id)
        {
            try
            {
                var termination = _employeeTerminationService.GetTerminationById(id);
                return Ok(termination);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task< IActionResult> UpdateTermination([FromForm] int terminationId, [FromForm] TerminationPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var result = await _employeeTerminationService.UpdateTermination(terminationId, model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> DeleteTermination(DeleteRequestVM model)
        {
            try
            {
                var result = await _employeeTerminationService.DeleteTermination(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        #region on chng

        public JsonResult GetEmployeesByCompany(int companyId)
        {
            var employees = _employeeOffiRepository.AllActive().Include(r => r.Employee)
                .Where(e => e.OrganizationID == companyId)
                .Select(e => new
                {
                    id = e.EmployeeID.ToString(),
                    name = e.Employee.FirstName + " " + e.Employee.LastName,
                }).ToList();

            return Json(employees);
        }

        #endregion
    }
}