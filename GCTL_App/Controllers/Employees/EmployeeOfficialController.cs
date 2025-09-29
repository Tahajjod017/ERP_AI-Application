using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
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

                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

                if (!hasEmployeePermission)
                {
                    var empid = loggedUser.EmployeeId;

                    if (empid == null || empid == 0)
                    {
                        return View();

                    }
                    else
                    {
                        EmployeeOfficialPostViewModel model = GetEmployeeDetailsMethod((int)loggedUser.EmployeeId);
                        return View(model);
                    }

                   
                }
                else {
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
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.ImmediateSupervisorDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
                "EmployeeID",
                "FullName"
            );

            ViewBag.HeadOfDepartmentDD = new SelectList(
                _employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }),
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

                    return Ok(result);
                }
                else
                {
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

                
            }

            return Ok(new { success = false, message = "ModelState Is Not Valid!" });
           
        }


        #endregion

        #region
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
            var branches = _branchRepository.AllActive().Where(e => e.OrganizationID == id).Select(r => new { r.OrganizationBranchID, r.OrganizationBranchName }).ToList();
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
            var empPersonal = _employeeRepository.AllActive().FirstOrDefault(e => e.EmployeeID == id);
            var empOfficial = _employeeOfficialRepository.AllActive().FirstOrDefault(e => e.EmployeeID == id);

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
                model.OrganizationID = empOfficial.OrganizationID;
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
                model.EmploymentStatusId = empOfficial.EmploymentStatusId;
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
        public IActionResult GetEmployeeSupDDbyComp(int id, int empID)
        {
            //var result = _employeeRepository.AllActive()
            //    .Where(e => e.EmployeeID != empID && e.EmployeeOfficeInfoEmployee.OrganizationId == id)
            //    .Include(e => e.EmployeeOfficeInfoEmployee)
            //    .Select(e => new
            //    {
            //        id = e.EmployeeID,
            //        FullName = $"{e.FirstName} {e.LastName}"
            //    })
            //    .ToList();

            //return Ok(result);


            var employeeList = (from emp in _employeeRepository.AllActive()
                                join office in _employeeOfficialRepository.AllActive()
                                    on emp.EmployeeID equals office.EmployeeID into officeJoin
                                from official in officeJoin.DefaultIfEmpty()
                                where emp.EmployeeID != empID
                                      && (official == null || official.OrganizationID == id)
                                select new
                                {
                                    id = emp.EmployeeID,
                                    FullName = emp.FirstName + " " + emp.LastName,
                                    
                                }).ToList();
            return Ok(employeeList);
        }

        [HttpGet]
        public IActionResult GetEmployeeHOD(int id)
        {

            var a = _departmentRepository.AllActive().Where(e=>e.DepartmentID == id).Select(u=>u.DepartmentHeadEmpID).FirstOrDefault();

            //var a = _employeeRepository.AllActive().Where(e => e.EmployeeID != id).Select(e => new { id = e.EmployeeID, FullName = e.FirstName + " " + e.LastName }).ToList();

            return Ok(a);
        }

        #endregion
    }
}
