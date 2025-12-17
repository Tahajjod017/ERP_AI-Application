using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers.AttendanceManagement.AttentendceReports.DailyReport;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeOfficialController : BaseController
    {

        #region CTOR

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<GCTL.Data.Models.OrganizationBranches> _branchRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<EmployeeType> _employeeTypeRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<EmploymentNature> _employmentNatureRepository;
        private readonly IGenericRepository<Statuses> _employeeStatusRepository;
        private readonly IGenericRepository<ProvisionPeriodTtimeTypes> _provisionPeriodTtimeTypesRepository;
        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly IGenericRepository<UserManager<ApplicationUser>> _userManagerRepository;
        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<RoleManager<ApplicationRole>> _roleManagerRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;

        private readonly IEmployeeOfficialService _employeeOfficialService;
        private readonly IElementPermissionService _elementPermissionService;
        private readonly AppDbContext _Db;
        private readonly UserManager<ApplicationUser> _userManager;


        public EmployeeOfficialController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<OrganizationBranches> branchRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<EmployeeType> employeeTypeRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<EmploymentNature> employmentNatureRepository, IGenericRepository<Statuses> employeeStatusRepository, IEmployeeOfficialService employeeOfficialService, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<ProvisionPeriodTtimeTypes> provisionPeriodTtimeTypesRepository, IEmployeeNavigationService employeeNavigationService, IGenericRepository<UserManager<ApplicationUser>> userManagerRepository, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<RoleManager<ApplicationRole>> roleManagerRepository, RoleManager<ApplicationRole> roleManagerRepository2, IGenericRepository<RoleModulePermissions> rolePermissionRepository, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IElementPermissionService elementPermissionService, AppDbContext db, UserManager<ApplicationUser> userManager) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _organizationRepository = organizationRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _employmentNatureRepository = employmentNatureRepository;
            _employeeStatusRepository = employeeStatusRepository;
            _employeeOfficialService = employeeOfficialService;
            _employeeOfficialRepository = employeeOfficialRepository;
            _provisionPeriodTtimeTypesRepository = provisionPeriodTtimeTypesRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository = userManagerRepository;
            _userManagerRepository2 = userManagerRepository2;
            _roleManagerRepository = roleManagerRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _rolePermissionRepository = rolePermissionRepository;
            _menuTabRepository = menuTabRepository;
            _elementPermissionService = elementPermissionService;
            _Db = db;
            _userManager = userManager;
        }

        #endregion


        #region Index 
        public async Task< IActionResult> Index(int id)
        {
            ViewBagData();
            SetSmartPageCode(112000);


           

            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.AllActive()
                                join mt in _menuTabRepository.AllActive() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "OfficialInfo");
                ViewBag.Navigation = navigationModel;

                var pageId = 2; 
                var elementKey = "EmployeeDropDown";

                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);

                if (!hasEmployeePermission)
                {
                    var empid = loggedUser.EmployeeId;

                    if (empid == null || empid == 0)
                    {
                        return View();

                    }
                    else
                    {

                        ViewBag.empId = empid;

                        EmployeeOfficialPostViewModel model = GetEmployeeDetailsMethod((int)loggedUser.EmployeeId);
                        return View(model);
                    }

                   
                }
                else {

                    ViewBag.empId = id;

                    EmployeeOfficialPostViewModel model = GetEmployeeDetailsMethod(id);
                    return View(model);
                }

                

                

                
            }


            return View();
        }

        private void ViewBagData()
        {
            #region ViewBagData

            ViewBag.EmployeeDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.OrganizationDD = new SelectList(
                _organizationRepository.AllActive().Select(o => new { o.OrganizationID, o.OrganizationName }),
                "OrganizationID",
                "OrganizationName"
            );

            ViewBag.BranchDD = new SelectList(
                _branchRepository.AllActive().Select(b => new { b.OrganizationBranchID, b.OrganizationBranchName }),
                "OrganizationBranchID",
                "OrganizationBranchName"
            );

            ViewBag.EmployeeTypeDD = new SelectList(
                _employeeTypeRepository.AllActive().Select(et => new { et.EmployeeTypeID, et.EmployeeTypeName }),
                "EmployeeTypeID",
                "EmployeeTypeName"
            );

            ViewBag.DepartmentDD = new SelectList(
                _departmentRepository.AllActive().Select(d => new { d.DepartmentID, d.DepartmentName }),
                "DepartmentID",
                "DepartmentName"
            );

            ViewBag.DesignationDD = new SelectList(
                _designationRepository.AllActive().Select(d => new { d.DesignationID, d.DesignationName }),
                "DesignationID",
                "DesignationName"
            );

            ViewBag.EmploymentNatureDD = new SelectList(
                _employmentNatureRepository.AllActive().Select(en => new { en.EmploymentNatureID, en.EmploymentNatureName }),
                "EmploymentNatureID",
                "EmploymentNatureName"
            );

            ViewBag.SeniorSupervisorDD = new SelectList(
                _employeeRepository.AllActive().Take(50).Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.ImmediateSupervisorDD = new SelectList(
                _employeeRepository.AllActive().Take(50).Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.HeadOfDepartmentDD = new SelectList(
                _employeeRepository.AllActive().Take(50).Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.EmployeeStatusDD = new SelectList(
                _employeeStatusRepository.AllActive().Where(e=>e.StatusType.ToLower() == "Active/Inactive".ToLower()).Select(es => new { es.StatusID, es.StatusName }),
                "StatusID",
                "StatusName"
            );


            ViewBag.TimeUnitDD = new SelectList(_provisionPeriodTtimeTypesRepository.AllActive().Select(tu => new { tu.ProvisionPeriodTtimeTypeID, tu.ProvisionPeriodTtimeTypeName }), "ProvisionPeriodTtimeTypeID", "ProvisionPeriodTtimeTypeName");


            //ViewBag.TimeUnitDD = new SelectList(new List<object>{
            //    new { TimeUnitID = 1, TimeUnitName = "Days" },
            //    new { TimeUnitID = 2, TimeUnitName = "Months" },
            //    new { TimeUnitID = 3, TimeUnitName = "Years" }
            //}, "TimeUnitID", "TimeUnitName");



            #endregion

        }

        [HttpPost]
        public async Task< IActionResult> Index(EmployeeOfficialPostViewModel model)
        {
            if (model.DepartmentID != null)
            {
                var head = await _departmentRepository.AllActive().FirstOrDefaultAsync(d => d.DepartmentID == model.DepartmentID);
                model.HeadOfDepartmentId = head?.DepartmentHeadEmpID;
            }
                

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );

                //var messages = ModelState
                //    .Where(x => x.Value.Errors.Count > 0)
                //    .SelectMany(x => x.Value.Errors)
                //    .Select(e => e.ErrorMessage)
                //    .ToList();

                var messages = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();


                return Json(new { success = false, errors = errors, message = messages });
            }

            if (ModelState.IsValid)
            {

                

                var chkDuplicate = await _employeeOfficialService.CheckValidEmployeeInfo(model);

                if (!chkDuplicate.Success)
                {
                    return Ok(chkDuplicate);
                }


                if (model.EmployeeOfficeInfoID == 0 || model.EmployeeOfficeInfoID == null)
                {
                    var result = await _employeeOfficialService.SaveEmployeeOfficialInfo(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }
                    if (result.Success)
                    {
                        await SyncUserEmailFromEmployeeAsync(model.EmployeeOfficeInfoID);
                    }

                    return Ok(result);
                }
                else
                {
                    var result = await _employeeOfficialService.UpdateEmployeeOfficialInfo(model);

                    if (!result.Success)
                    {
                        return Ok(result);
                    }
                    

                    return Ok(result);
                }

                
            }

           

            return Ok(new { success = false, message = "ModelState Is Not Valid!" });
           
        }


        #endregion


        #region Get Supervisors
        [HttpGet]
        public async Task<IActionResult> GetSupervisors(string search = "", int page = 1, int pageSize = 10,
            string roleType = "", int organizationId = 0, int departmentId = 0)
        {
            try
            {
                // Validate organization ID
                if (organizationId == 0)
                {
                    return Ok(new
                    {
                        results = new List<object>(),
                        pagination = new { more = false }
                    });
                }

                var result = await _employeeOfficialService.GetPagedSupervisorsAsync(
                    search, page, pageSize, organizationId, departmentId, roleType
                );

                var more = (page * pageSize) < result.totalItem;

                var formatted = new
                {
                    results = result.data.Select(s => new
                    {
                        id = s.EmployeeId,
                        text = $"{s.FullName} - {s.Department} - {s.Position}",
                        department = s.Department,
                        position = s.Position,
                        email = s.Email
                    }),
                    pagination = new { more }
                };

                return Ok(formatted);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        #endregion

        #region SyncUserEmailFromEmployeeAsync
        private async Task<JsonResult> SyncUserEmailFromEmployeeAsync(int? employeeOfficeInfoId)
        {
            try
            {
                // Load the employee
                var employee = await _employeeOfficialRepository
                    .FirstOrDefaultAsync(e => e.EmployeeOfficeInfoID == employeeOfficeInfoId && e.DeletedAt == null);

                if (employee == null)
                    return new JsonResult(new { success = false, message = "Employee not found." });

                // Find the linked Identity user
                var appUser = await _Db.Users.FirstOrDefaultAsync(u => u.EmployeeId == employee.EmployeeID);
                if (appUser == null)
                    return new JsonResult(new { success = true, message = "No linked identity user. Skipped email sync." });

                // Source of truth for email (same fallback you use on create)
                var newEmail = string.IsNullOrWhiteSpace(employee.OfficeEmail)
                    ? "default@gmail.com"
                    : employee.OfficeEmail.Trim();

                // If nothing changed, bail early
                var sameEmail = string.Equals(appUser.Email, newEmail, StringComparison.OrdinalIgnoreCase);
                var sameUserName = string.Equals(appUser.UserName, newEmail, StringComparison.OrdinalIgnoreCase);
                if (sameEmail && sameUserName)
                    return new JsonResult(new { success = true, message = "No changes needed." });

                // Ensure the email isn't already taken by a different Identity user
                var existingByEmail = await _userManager.FindByEmailAsync(newEmail);
                if (existingByEmail != null && existingByEmail.Id != appUser.Id)
                    return new JsonResult(new
                    {
                        success = false,
                        message = "That email address is already in use by another account.",
                        errors = new[] { "Email already taken." }
                    });

                var allErrors = new List<string>();

                // If you require reconfirmation when an email changes:
                appUser.EmailConfirmed = false;

                // Update Email via UserManager (handles normalization)
                var setEmail = await _userManager.SetEmailAsync(appUser, newEmail);
                if (!setEmail.Succeeded)
                    allErrors.AddRange(setEmail.Errors.Select(e => e.Description));

                // If you use email as username too, update it as well
                var setUserName = await _userManager.SetUserNameAsync(appUser, newEmail);
                if (!setUserName.Succeeded)
                    allErrors.AddRange(setUserName.Errors.Select(e => e.Description));

                if (allErrors.Count > 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Failed to sync user email/username.",
                        errors = allErrors
                    });
                }

                // Optional: rotate security stamp
                await _userManager.UpdateSecurityStampAsync(appUser);

                // Optional: trigger a confirmation email flow here if you have it wired
                // await _emailSender.SendEmailConfirmationAsync(appUser, tokenLink);

                return new JsonResult(new { success = true, message = "User email/username synced successfully." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Form Edit page

        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeOfficialPostViewModel model)
        {
            //return Ok(model);

            var chkDuplicate = await _employeeOfficialService.CheckValidEmployeeInfo(model);

            if (!chkDuplicate.Success)
            {
                return Ok(chkDuplicate);
            }

            var result = await _employeeOfficialService.UpdateEmployeeOfficialInfo(model);

            if (!result.Success)
            {

                return Ok(result);
            }
            if (result.Success)
            {
                
                await SyncUserEmailFromEmployeeAsync(model.EmployeeOfficeInfoID);
            }
            return Ok(result);

        }

        #endregion

        #region Get Branches

        public IActionResult GetBranches(int id)
        {
            var branches = _branchRepository.AllActive().Where(e => e.OrganizationID == id).Select(r => new {id = r.OrganizationBranchID, name = r.OrganizationBranchName }).ToList();
            return Ok(branches);
        }

        #endregion

        #region GetEmployeeDetails

        [HttpGet]
        public IActionResult GetEmployeeDetails(int id)
        {
           var model =  GetEmployeeDetailsMethod(id);

            return Ok(model);
        }


        
        private EmployeeOfficialPostViewModel GetEmployeeDetailsMethod(int id)
        {
            var empPersonal = _employeeRepository.All().FirstOrDefault(e => e.EmployeeID == id);
            var empOfficial = _employeeOfficialRepository.All().FirstOrDefault(e => e.EmployeeID == id);

            EmployeeOfficialPostViewModel model = new EmployeeOfficialPostViewModel();

            if (empPersonal != null)
            {
                model.EmployeePersonalId = empPersonal.EmployeeID;
                model.PersonalEmail = empPersonal.Email;
                model.PersonalPhone = empPersonal.MobileNumber;
            }

            if (empOfficial != null)
            {
                model.EmployeeOfficeId = empOfficial.EmployeeOfficeId ?? string.Empty;
                model.EmployeeOfficeInfoID = empOfficial.EmployeeOfficeInfoID;
                model.OrganizationID = empOfficial.OrganizationID ?? 0;
                model.OrganizationBranchID = empOfficial.OrganizationBranchID;
                model.DepartmentID = empOfficial.DepartmentID;
                model.DesignationID = empOfficial.DesignationID;
                model.EmployeeTypeID = empOfficial.EmployeeTypeID;
                model.EmploymentNatureID = empOfficial.EmploymentNatureID;
                model.SeniorSupervisorId = empOfficial.SeniorSupervisorId;
                model.ImmediateSupervisorId = empOfficial.ImmediateSupervisorId;
                model.HeadOfDepartmentId = empOfficial.HeadOfDepartmentId;
                model.OfficePhone = empOfficial.OfficePhone ?? string.Empty;
                model.OfficeEmail = empOfficial.OfficeEmail ?? string.Empty;
                model.AttendanceId = empOfficial.AttendanceId ?? string.Empty;
                model.EmploymentStatusId = empOfficial.EmploymentStatusId ?? 0;
                model.AppointmentLetterNo = empOfficial.AppointmentLetterNo ?? string.Empty;
                model.AppointmentLetterIssueDate = empOfficial.AppointmentLetterIssueDate;
                model.JoiningDate = empOfficial.JoiningDate;
                model.ProvisionPeriodStartDate = empOfficial.ProvisionPeriodStartDate;
                model.ProvisionPeriod = empOfficial.ProvisionPeriod;
                model.ProvisionPeriodTtimeTypeID = empOfficial.ProvisionPeriodTtimeTypeID;
                model.ConfirmationDate = empOfficial.ConfirmationDate;
                model.ConfirmationLetterNo = empOfficial.ConfirmationLetterNo ?? string.Empty;
                model.ContractEndDate = empOfficial.ContractEndDate;
            }

            return model;
        }

        #endregion

        #region Get For DD

        [HttpGet]
        public IActionResult GetEmployeeSupDD(int id)
        {
            var a = _employeeRepository.AllActive().Where(e=>e.EmployeeID != id).Select(e => new { id =  e.EmployeeID, FullName = e.FirstName + " " + e.LastName }).ToList();
                
            return Ok(a);
        }



       

        [HttpGet]
        public IActionResult GetEmployeeHOD(int id, string search = "", int page = 1, int pageSize = 50)
        {
            try
            {
                var employees = (from office in _employeeOfficialRepository.AllActive()
                                 join emp in _employeeRepository.AllActive()
                                     on office.EmployeeID equals emp.EmployeeID
                                 where office.DepartmentID == id
                                       && (string.IsNullOrEmpty(search) || (emp.FirstName + " " + emp.LastName).Contains(search, StringComparison.OrdinalIgnoreCase))
                                 select new
                                 {
                                     value = emp.EmployeeID,
                                     label = emp.FirstName + " " + emp.LastName
                                 })
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                var totalCount = _employeeOfficialRepository.AllActive()
                    .Count(e => e.DepartmentID == id);

                return Ok(new
                {
                    items = employees,
                    hasMore = totalCount > (page * pageSize)
                });
            }
            catch (Exception)
            {

                throw;
            }
           
        }


        #endregion


        #region Get Head By Department Id

        [HttpGet]
        public async Task<IActionResult> GetHeadById(int id)
        {
            // Example: fetch department head from database
            var head = await _departmentRepository.AllActive().Include(e=>e.DepartmentHeadEmp)
                .Where(d => d.DepartmentID == id)
                .Select(d => new
                {
                    Id = d.DepartmentHeadEmpID,
                    Name = d.DepartmentHeadEmp.FirstName + " " + d.DepartmentHeadEmp.LastName
                })
                .FirstOrDefaultAsync();

            if (head == null)
            {
                return NotFound();
            }

            return Json(head);
        }

        #endregion

        private async Task<IQueryable<GCTL.Data.Models.Employees>> ApplyPermissionFilter(IQueryable<GCTL.Data.Models.Employees> query)
        {
            var loggedUser = await _userManagerRepository2.GetUserAsync(User);
            if (loggedUser == null) return query;

            var userId = loggedUser.Id;
            var user = await _userManagerRepository2.FindByIdAsync(userId);
            var roleNames = await _userManagerRepository2.GetRolesAsync(user);
            var roleIds = _roleManagerRepository2.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToList();

            var pageId = 2;
            var elementKey = "EmployeeDropDown";

            bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);


           // bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

            if (!hasEmployeePermission)
            {
                var empid = loggedUser.EmployeeId;
                query = query.Where(e => e.EmployeeID == empid);
            }

            return query;
        }


        [HttpGet]
        public async Task<IActionResult> GetEmployeeSupDDbyComp(    string search = "", int page = 1, int pageSize = 50,    int? organizationId = null, int? employeePersonalId = null)
        {
            try
            {
                var query = _employeeRepository.AllActive().Include(e=>e.EmployeeOfficeInfoEmployee).AsQueryable();

                // ---- Permission filter (same as employee) ----
                query = await ApplyPermissionFilter(query);

                // ---- Optional filters (sent from the front-end) ----
                if (organizationId.HasValue)
                {
                    query = query.Where(e => e.EmployeeOfficeInfoEmployee.FirstOrDefault().OrganizationID == organizationId.Value);
                }

                if (employeePersonalId.HasValue)
                {
                    // Example: exclude the selected employee himself
                    query = query.Where(e => e.EmployeeID != employeePersonalId.Value);
                }

                // ---- Search term ----
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(e =>
                        e.FirstName.ToLower().Contains(search) ||
                        e.LastName.ToLower().Contains(search) ||
                        e.EmployeeCode.ToLower().Contains(search) ||
                        e.Email.ToLower().Contains(search));
                }

                query = query.OrderBy(e => e.FirstName);

                var totalCount = await query.CountAsync();
                var hasMore = (page * pageSize) < totalCount;

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        value = e.EmployeeID.ToString(),
                        label = $"{e.FirstName} {e.LastName} ({e.EmployeeCode})"
                    })
                    .ToListAsync();

                return Json(new
                {
                    items,
                    hasMore,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    items = new List<object>(),
                    hasMore = false,
                    error = "Failed to load supervisors"
                });
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetEmployeeHOD(    string search = "", int page = 1, int pageSize = 50,    int? departmentId = null)
        {
            try
            {
                var query = _employeeRepository.AllActive().Include(e=>e.EmployeeOfficeInfoEmployee).AsQueryable();

                query = await ApplyPermissionFilter(query);

                if (departmentId.HasValue)
                {
                    // Assuming you have a navigation property or a join table
                    // Example: Employee.DepartmentID
                    query = query.Where(e => e.EmployeeOfficeInfoEmployee.FirstOrDefault().DepartmentID == departmentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(e =>
                        e.FirstName.ToLower().Contains(search) ||
                        e.LastName.ToLower().Contains(search) ||
                        e.EmployeeCode.ToLower().Contains(search));
                }

                query = query.OrderBy(e => e.FirstName);

                var totalCount = await query.CountAsync();
                var hasMore = (page * pageSize) < totalCount;

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        value = e.EmployeeID.ToString(),
                        label = $"{e.FirstName} {e.LastName} ({e.EmployeeCode}) - HOD"
                    })
                    .ToListAsync();

                return Json(new
                {
                    items,
                    hasMore,
                    totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    items = new List<object>(),
                    hasMore = false,
                    error = "Failed to load HOD"
                });
            }
        }





    }
}
