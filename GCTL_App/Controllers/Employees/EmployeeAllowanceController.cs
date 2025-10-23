using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Core.ViewModels.Employee.EmployeeOfficial;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
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

        private readonly IGenericRepository<Percentages> percentagesService;
        public EmployeeAllowanceController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IEmployeeAllowanceService employeeAllowanceService, IEmployeeNavigationService employeeNavigationService, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<Organization> organizationRepository, IElementPermissionService elementPermissionService, IGenericRepository<Percentages> percentagesService) : base(translateService, userProfileService)
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
            this.percentagesService = percentagesService;
        }

        public async Task< IActionResult> Index(int id)
        {

            


            SetSmartPageCode(119000);


            var loggedUser = await _userManagerRepository2.GetUserAsync(User);
            ViewBag.OrganizationDD = new SelectList(
                _organizationRepository.AllActive().Select(o => new { o.OrganizationID, o.OrganizationName }),
                "OrganizationID",
                "OrganizationName"
            );
            ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");
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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PayrollInfo", "EmployeeAllowance" );
                ViewBag.Navigation = navigationModel;

                var pageId = 2;
                var elementKey = "EmployeeDropDown";

                bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, pageId, elementKey);


               // bool hasEmployeePermission = await _elementPermissionService.HasPermissionForElementAsync(userId, 2, "EmployeeTable");

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


        #region Get Allowance Type according to Organization
        [Route("EmployeeAllowance/SelectAsync")]
        [HttpGet]
        public async Task<IActionResult> SelectAsync(int id)
        {
            try
            {
                var data = await _employeeAllowanceService.SelectAsync(id);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion
        [Route("EmployeeAllowance/Save")]
        [HttpPost]
        public async Task<IActionResult> Save(EmployeeAlowancePostViewModel22 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeAllowanceService.Save(model);

            if (!result.Success) return BadRequest(result.Message);

            return Json(new { Success = result.Success, Message = result.Message });
        }



    }
}
