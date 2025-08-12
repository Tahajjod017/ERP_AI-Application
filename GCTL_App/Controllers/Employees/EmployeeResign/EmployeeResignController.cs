using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeResign;
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

        public EmployeeResignController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository,
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Designations> designationRepository,
            IEmployeeResign employeeResignService) : base(translateService, userProfileService)
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

            // Static employee dropdown data
            ViewBag.EmployeeDD = new SelectList(new[]
            {
                new { id = 1, name = "Tanvir Haider" },
                new { id = 2, name = "Hasan Tarek" },
                new { id = 3, name = "Osman Goni" },
                new { id = 4, name = "Jashim Uddin" },
                new { id = 5, name = "Ahmed Rahman" },
                new { id = 6, name = "Sarah Khan" }
            }, "id", "name");

            // Static organization dropdown data
            ViewBag.OrganizationDD = new SelectList(new[]
            {
                new { OrganizationID = 1, OrganizationName = "GCTL Head Office" },
                new { OrganizationID = 2, OrganizationName = "GCTL Branch Office" },
                new { OrganizationID = 3, OrganizationName = "GCTL Remote Office" }
            }, "OrganizationID", "OrganizationName");

            return View();
        }

        [HttpGet]
        public IActionResult GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate = null, string toDate = null)
        {
            var result = _employeeResignService.GetResignations(page, pageSize, sortColumn, sortDirection, fromDate, toDate);
            return Json(result);
        }

        [HttpPost]
        public IActionResult CreateResignation([FromForm] ResignationPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var result = _employeeResignService.InsertResignation(model);

                if (result)
                {
                    return Json(new { success = true, message = "Resignation added successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add resignation." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateResignation([FromForm] int resignationId, [FromForm] ResignationPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var result = _employeeResignService.UpdateResignation(resignationId, model);

                if (result)
                {
                    return Json(new { success = true, message = "Resignation updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update resignation. Record not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetResignation(int id)
        {
            try
            {
                var resignation = _employeeResignService.GetResignationById(id);
                if (resignation != null)
                {
                    return Json(new { success = true, data = resignation });
                }
                else
                {
                    return Json(new { success = false, message = "Resignation not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteResignation(int id)
        {
            try
            {
                var result = _employeeResignService.DeleteResignation(id);

                if (result)
                {
                    return Json(new { success = true, message = "Resignation deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete resignation. Record not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}