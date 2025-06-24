using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeAllowanceController : BaseController
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        
        private readonly IEmployeeAllowanceService _employeeAllowanceService;

        private readonly IEmployeeNavigationService _employeeNavigationService;

        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;

        private readonly IGenericRepository<Organization> _organizationRepository;

        private readonly IElementPermissionService _elementPermissionService;


        public EmployeeAllowanceController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeAllowanceService employeeAllowanceService, IEmployeeNavigationService employeeNavigationService, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<Organization> organizationRepository, IElementPermissionService elementPermissionService) : base(translateService, userProfileService)
        {
            _employeeRepository = employeeRepository;
            _employeeAllowanceService = employeeAllowanceService;
            _employeeNavigationService = employeeNavigationService;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _userManagerRepository2 = userManagerRepository2;
            _organizationRepository = organizationRepository;
            _elementPermissionService = elementPermissionService;
        }

        public async Task< IActionResult> Index(int id)
        {

            PopulateViewBag();


            SetSmartPageCode(119000);


            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); // Get the ApplicationUser
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); // List<string> of role names

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.All()
                                join mt in _menuTabRepository.All() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PayrollInfo", "EmployeeAllowance" );
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
                        var model = _employeeAllowanceService.GetEmployeeAllowance((int)empid).Result;

                        
                        return View(model);
                    }


                }
                else
                {
                    var model = _employeeAllowanceService.GetEmployeeAllowance(id).Result;

                    
                    return View(model);
                }
            }

            

           
            return View();
        }

        private void PopulateViewBag()
        {
            #region ViewBag


            ViewBag.OrganizationDD = new SelectList(
                _organizationRepository.All().Select(o => new { o.OrganizationID, o.OrganizationName }),
                "OrganizationID",
                "OrganizationName"
            );

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");


            ViewBag.HouseRentAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "35.00", Text = "35 %" },
                new SelectListItem { Value = "40.00", Text = "40 %" },
                new SelectListItem { Value = "45.00", Text = "45 %" },
                new SelectListItem { Value = "50.00", Text = "50 %" },
                new SelectListItem { Value = "60.00", Text = "60 %" },
                new SelectListItem { Value = "70.00", Text = "70 %" },
                new SelectListItem { Value = "100.00", Text = "100 %" }
            }, "Value", "Text");

            ViewBag.MedicalAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8.00", Text = "8 %" },
                new SelectListItem { Value = "10.00", Text = "10 %" },
                new SelectListItem { Value = "15.00", Text = "15 %" }
            }, "Value", "Text");

            ViewBag.ConveyanceAllowanceDD = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "8.00", Text = "8 %" },
                new SelectListItem { Value = "10.00", Text = "10 %" },
                new SelectListItem { Value = "15.00", Text = "15 %" }
            }, "Value", "Text");

            #endregion
        }

        private DateTime? ConvertToDateTime(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }





        [HttpPost]
        public async Task<IActionResult> Index(EmployeeAdditionalPostViewModel model)
        {
            if(model.MobileAllowanceEffectiveFromStr != null)
            model.MobileAllowanceEffectiveFrom = ConvertToDateTime(model.MobileAllowanceEffectiveFromStr);

            if (model.InternetAllowanceEffectiveFromStr != null)
                model.InternetAllowanceEffectiveFrom = ConvertToDateTime(model.InternetAllowanceEffectiveFromStr);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(errors);
            }

            try
            {
                var res = await _employeeAllowanceService.SaveEmployeeAllowanceAsync(model);

                return Ok(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFromEdit(EmployeeAdditionalPostViewModel model)
        {
            if (model.MobileAllowanceEffectiveFromStr != null)
                model.MobileAllowanceEffectiveFrom = ConvertToDateTime(model.MobileAllowanceEffectiveFromStr);

            if (model.InternetAllowanceEffectiveFromStr != null)
                model.InternetAllowanceEffectiveFrom = ConvertToDateTime(model.InternetAllowanceEffectiveFromStr);

           

            try
            {
                var res = await _employeeAllowanceService.SaveEmployeeAllowanceAsync(model);

                return Ok(res);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAllowance(int employeeId)
        {
            try
            {
                
                 var allowanceData = await _employeeAllowanceService.GetEmployeeAllowance(employeeId);

                // Return the data as JSON
                return Json(new
                {
                    employeePersonalId = allowanceData.EmployeePersonalId,
                    employeeBaseAllowanceID =  allowanceData?.EmployeeBaseAllowanceID ?? 0,
                    personalEmail = allowanceData.PersonalEmail ?? "",
                    personalPhone = allowanceData.PersonalPhone,

                    organizationID = allowanceData.OrganizationID,

                    //mobileInternetAllowance = allowanceData?.MobileInternetAllowance,
                    //isMobileInternetAllowanceEnabled =  allowanceData?.IsMobileInternetAllowanceEnabled ?? false,
                    mobileAllowance = allowanceData?.MobileAllowance,
                    internetAllowance = allowanceData?.InternetAllowance,
                    isMobileAllowanceEnabled = allowanceData?.IsMobileAllowanceEnabled ?? false,
                    isInternetAllowanceEnabled = allowanceData?.IsInternetAllowanceEnabled ?? false,
                    mobileAllowanceEffectiveFrom = allowanceData?.MobileAllowanceEffectiveFrom?.ToString("dd/MM/yyyy"),
                    internetAllowanceEffectiveFrom = allowanceData?.InternetAllowanceEffectiveFrom?.ToString("dd/MM/yyyy"),


                    shiftAllowance =allowanceData?.ShiftAllowance,
                    isShiftAllowanceEnabled =  allowanceData?.IsShiftAllowanceEnabled ?? false,
                    houseRentAllowancePercentage =  allowanceData?.HouseRentAllowancePercentage,
                    isHouseRentAllowancePercentageEnabled =  allowanceData?.IsHouseRentAllowancePercentageEnabled ?? false,
                    medicalAllowancePercentage =  allowanceData?.MedicalAllowancePercentage,
                    isMedicalAllowancePercentageEnabled =  allowanceData?.IsMedicalAllowancePercentageEnabled ?? false,
                    conveyanceAllowancePercentage =  allowanceData?.ConveyanceAllowancePercentage,
                    isConveyanceAllowancePercentageEnabled =  allowanceData?.IsConveyanceAllowancePercentageEnabled ?? false,
                    isEmployeeAllowanceEnabled =  allowanceData?.IsEmployeeAllowanceEnabled ?? false
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
