using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GCTL_App.Controllers.Employees.EmployeeResign
{
    public class EmployeeResignController : BaseController
    {
        #region CTOR
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

        #endregion

        #region Index N Table load

        public IActionResult Index()
        {
            SetSmartPageCode(1196500);


            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new
            {
               id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }) , "id" , "name");
            
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");

            return View();
        }

        [HttpGet]
        public IActionResult GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate = null, string toDate = null)
        {
            

            var result = _employeeResignService.GetResignations(page, pageSize, sortColumn, sortDirection, fromDate, toDate , imgSrcThumb);
            return Json(result);
        }

        #endregion


        #region Create 

        [HttpPost]
        public async Task< IActionResult> CreateResignation([FromForm] ResignationPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data provided." });
                }

                var result = await _employeeResignService.InsertResignation(model);

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
        public IActionResult GetResignation(int id)
        {
            try
            {
                var resignation = _employeeResignService.GetResignationById(id);
                return Ok(resignation);
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        #endregion

        [HttpPost]
        public IActionResult DeleteResignation(DeleteRequestVM model)
        {
            try
            {
                
                var result = _employeeResignService.DeleteResignation( model);

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
            var employees = _employeeOffiRepository.AllActive().Include(r=>r.Employee)
                .Where(e => e.OrganizationID  == companyId)
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