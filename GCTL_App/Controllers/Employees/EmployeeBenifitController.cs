using System.Drawing;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeBenifitController : BaseController
    {
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

        public EmployeeBenifitController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IEmployeeBenifitService employeeBenifitService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypesRepository, IGenericRepository<ServiceYears> serviceYearsRepository, IEmployeeNavigationService employeeNavigationService, UserManager<ApplicationUser> userManagerRepository2, IGenericRepository<GCTL.Data.Models.MenuTab> menuTabRepository, IGenericRepository<RoleModulePermissions> rolePermissionRepository, RoleManager<ApplicationRole> roleManagerRepository2) : base(translateService, userProfileService)
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
        }

        public async Task< IActionResult> Index(int id)
        {
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


                var navigationModel = _employeeNavigationService.GetEmployeeNavigation(menuTabs, "PayrollInfo" , "EmployeeBenefits");
                ViewBag.Navigation = navigationModel;
            }

          

            var model = await _employeeBenifitService.GetEmployeeBenefitsAsync(id.ToString());

            PopulateViewBag();



            SetSmartPageCode(118000);
            return View(model);
        }

        private void PopulateViewBag()
        {
            #region Voriwe Bag

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            ViewBag.YearlyEndBonusTypeDD = new SelectList(_yearlyEndBonusTypesRepository.All().Select(e => new { e.YearlyEndBonusTypeID, e.YearlyEndBonusTypeName }), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");

            ViewBag.ServiceYearDD = new SelectList(_serviceYearsRepository.All().Select(e => new { e.ServiceYearID, e.ServiceYearName }), "ServiceYearID", "ServiceYearName");

            ViewBag.FastivalBonusPercentageDD = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "35.00", Text = "35 %" },
                    new SelectListItem { Value = "40.00", Text = "40 %" },
                    new SelectListItem { Value = "45.00", Text = "45 %" },
                    new SelectListItem { Value = "50.00", Text = "50 %" },
                    new SelectListItem { Value = "60.00", Text = "60 %" },
                    new SelectListItem { Value = "70.00", Text = "70 %" },
                    new SelectListItem { Value = "100.00", Text = "100 %" }
                }, "Value", "Text");

            ViewBag.BonusDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");


            ViewBag.PFEmployeeContributionDD = new SelectList(new[]
                {
                    new { Value = "5.00", Text = "5 %" },
                    new { Value = "6.00", Text = "6 %" },
                    new { Value = "7.00", Text = "7 %" },
                    new { Value = "8.00", Text = "8 %" },
                    new { Value = "9.00", Text = "9 %" },
                    new { Value = "10.00", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFOrgContributionDD = new SelectList(new[]
                {
                    new { Value = "5.00", Text = "5 %" },
                    new { Value = "6.00", Text = "6 %" },
                    new { Value = "7.00", Text = "7 %" },
                    new { Value = "8.00", Text = "8 %" },
                    new { Value = "9.00", Text = "9 %" },
                    new { Value = "10.00", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");


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
                    employeeBaseBenefitID = benefit.EmployeeBaseBenefitID,
                    employeePersonalId = benefit.EmployeePersonalId,
                    personalEmail = benefit.PersonalEmail,
                    personalPhone = benefit.PersonalPhone,
                    isBenifitEnabled = benefit.IsBenifitEnabled,
                    healthInsurance = benefit.HealthInsurance,
                    isHealthInsuranceEnabled = benefit.IsHealthInsuranceEnabled,
                    performanceBonus = benefit.PerformanceBonus,
                    isPerformanceBonusEnabled = benefit.IsPerformanceBonusEnabled,
                    yearlyEndBonusTypeID = benefit.YearlyEndBonusTypeID,
                    isYearlyEndBonusTypeIDEnabled = benefit.IsYearlyEndBonusTypeIDEnabled,
                    fastivalBonusPercentage = benefit.FastivalBonusPercentage,
                    isFastivalBonusPercentageEnabled = benefit.IsFastivalBonusPercentageEnabled,
                    providantFundEmployeePercentage = benefit.ProvidantFundEmployeePercentage,
                    providantFundOrganizationPercentage = benefit.ProvidantFundOrganizationPercentage,
                    isProvidantFundEnabled = benefit.IsProvidantFundEnabled,
                    serviceYearID = benefit.ServiceYearID
                };

                return Json(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging framework, e.g., Serilog, NLog)
                return Json(new { success = false, message = "An error occurred while fetching employee benefits." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Index(EmployeeBenifitPostViewModel model)
        {
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
                var result =  await _employeeBenifitService.SaveOrUpdateEmployeeBenefitsAsync(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}