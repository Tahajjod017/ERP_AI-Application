using GCTL.Core.Repository;
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

<<<<<<< Updated upstream
=======



        [HttpGet]
        public IActionResult GetDetails(int id)
        {

            var result = _employeeResignService.GetToolTipData(id);


            
            var approvalSteps = new List<object>
            {
                new {
                    approverStep = "Stage 1",
                    statusName = "Pending",
                    approvarPerson = "Sarah Johnson (HR Manager)",
                    approvarNote = "Awaiting documentation",
                    approvedOrDeclineDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd")
                },
                new {
                    approverStep = "Stage 2",
                    statusName = "In Progress",
                    approvarPerson = "Michael Chen (Department Head)",
                    approvarNote = "Under review - checking handover status",
                    approvedOrDeclineDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")
                },
                //new {
                //    approverStep = "Stage 3",
                //    statusName = "Not Started",
                //    approvarPerson = "Lisa Rodriguez (Finance Director)",
                //    approvarNote = "Pending previous approvals",
                //    approvedOrDeclineDate = "Not yet started"
                //}
            };

           

            return Json(result);
        }

>>>>>>> Stashed changes
        #region Index N Table load

        public IActionResult Index()
        {
            SetSmartPageCode(111800);


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


        #endregion

        [HttpPost]
        public IActionResult DeleteResignation(int id)
        {
            try
            {
                var result = _employeeResignService.DeleteResignation(id);

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