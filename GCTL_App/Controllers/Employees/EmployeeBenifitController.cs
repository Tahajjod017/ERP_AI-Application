using System.Drawing;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeBenifitController : BaseController
    {
        #region CTOR
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeBaseBenefits> _employeeBenifitRepository;
        private readonly IGenericRepository<YearlyEndBonusTypes> _yearlyEndBonusTypesRepository;
        private readonly IGenericRepository<ServiceYears> _serviceYearsRepository;
        private readonly IEmployeeBenifitService _employeeBenifitService;
        private readonly IEmployeeNavigationService _employeeNavigationService;
        private readonly UserManager<ApplicationUser> _userManagerRepository2;
        private readonly IGenericRepository<GCTL.Data.Models.MenuTab> _menuTabRepository;
        private readonly IGenericRepository<RoleModulePermissions> _rolePermissionRepository;
        private readonly RoleManager<ApplicationRole> _roleManagerRepository2;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IElementPermissionService _elementPermissionService;
        private readonly IGenericRepository<Percentages> percentagesService;

        public EmployeeBenifitController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IEmployeeBenifitService employeeBenifitService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypesRepository, IGenericRepository<ServiceYears> serviceYearsRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2, IGenericRepository<Organization> organizationRepository, IElementPermissionService elementPermissionService, IGenericRepository<Percentages> percentagesService = null) : base(translateService, userProfileService)
        {
            _employeeBenifitRepository = employeeBenifitRepository;
            _employeeBenifitService = employeeBenifitService;
            _employeeRepository = employeeRepository;
            _yearlyEndBonusTypesRepository = yearlyEndBonusTypesRepository;
            _serviceYearsRepository = serviceYearsRepository;
            _employeeNavigationService = employeeNavigationService;
            _userManagerRepository2 = userManagerRepository2;
            _menuTabRepository = menuTabRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _roleManagerRepository2 = roleManagerRepository2;
            _organizationRepository = organizationRepository;
            _elementPermissionService = elementPermissionService;
            this.percentagesService = percentagesService;
        }

        #endregion

        public async Task< IActionResult> Index(int id)
        {
            PopulateViewBag();

            SetSmartPageCode(118000);

            var loggedUser = await _userManagerRepository2.GetUserAsync(User);

            if (loggedUser != null)
            {

                var userId = loggedUser.Id;

                var user = await _userManagerRepository2.FindByIdAsync(userId); 
                var roleNames = await _userManagerRepository2.GetRolesAsync(user); 

                var roleIds = _roleManagerRepository2.Roles.Where(role => roleNames.Contains(role.Name)).Select(role => role.Id).ToList();

                var menuTabs = (from rp in _rolePermissionRepository.AllActive()
                                join mt in _menuTabRepository.AllActive() on rp.MenuTabId equals mt.MenuTabId
                                where roleIds.Contains(rp.RoleId) && mt.ControllerName.StartsWith("Employee")
                                select mt.ControllerName).Distinct().ToList();


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PayrollInfo" , "EmployeeBenefits");
                ViewBag.Navigation = navigationModel;
                ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
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
                       
                        var model = await _employeeBenifitService.GetEmployeeBenefitsAsync(empid.ToString());


                        return View(model);
                    }


                }
                else
                {
                 
                    var model = await _employeeBenifitService.GetEmployeeBenefitsAsync(id.ToString());


                    return View(model);
                }
            }

           
            return View();
        }

        private void PopulateViewBag()
        {
            #region Voriwe Bag

            ViewBag.OrganizationDD = new SelectList(    _organizationRepository.AllActive().Select(o => new { o.OrganizationID, o.OrganizationName }),    "OrganizationID",    "OrganizationName");

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.AllActive().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            

            #endregion

        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeBenifitData(int employeeId)
        {
            var employeeBenifitData = await _employeeBenifitService.GetEmployeeBenefitsAsync(employeeId.ToString());
            return Ok(employeeBenifitData);

        }



        [HttpGet]
        public async Task<IActionResult> GetEmployeeBenefits(string employeeId)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    return Json(new { success = false, message = "Employee ID is required." });
                }

                var benefit = await _employeeBenifitService.GetEmployeeBenefitsAsync(employeeId);
                if (benefit == null)
                {
                    return Json(new { success = false, message = "No benefits found for the selected employee." });
                }

                // Map to view model
                var response = new
                {
                    organizationID = benefit.OrganizationID,

                    employeeBaseBenefitID = benefit.EmployeeBaseBenefitID,
                    employeePersonalId = benefit.EmployeePersonalId,
                    personalEmail = benefit.PersonalEmail,
                    personalPhone = benefit.PersonalPhone,
                   
                };

                return Json(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging framework, e.g., Serilog, NLog)
                return Json(new { success = false, message = ex.Message });
            }
        }



       

       

        #region Get Allowance Type according to Organization
        [Route("EmployeeBenifitController/SelectBenefitsTypeAsync")]
        [HttpGet]
        public async Task<IActionResult> SelectAsync(int id)
        {
            try
            {
                var data = await _employeeBenifitService.SelectAsync(id);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion
        
        [HttpPost]
        public async Task<IActionResult> Save(EmployeeBenifitPostViewModel22 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeBenifitService.SaveOrUpdateEmployeeBenefitsAsync1(model);

            if (!result.Success) return BadRequest(result.Message);

            return Json(new {Success=result.Success, Message=result.Message});
        }

    }
}


