using System.Drawing;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Language;
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
        public EmployeeBenifitController(ITranslateService translateService, IGenericRepository<EmployeeBaseBenefits> employeeBenifitRepository, IEmployeeBenifitService employeeBenifitService, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypesRepository, IGenericRepository<ServiceYears> serviceYearsRepository) : base(translateService)
        {
            _employeeBenifitRepository = employeeBenifitRepository;
            _employeeBenifitService = employeeBenifitService;
            _employeeRepository = employeeRepository;
            _yearlyEndBonusTypesRepository = yearlyEndBonusTypesRepository;
            _serviceYearsRepository = serviceYearsRepository;
        }

        public IActionResult Index()
        {
            #region Voriwe Bag

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new { e.EmployeeID, FullName = e.FirstName + " " + e.LastName }), "EmployeeID", "FullName");

            ViewBag.YearlyEndBonusTypeDD = new SelectList(_yearlyEndBonusTypesRepository.All().Select(e => new { e.YearlyEndBonusTypeID, e.YearlyEndBonusTypeName }), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");
            
            ViewBag.ServiceYearDD = new SelectList(_serviceYearsRepository.All().Select(e => new { e.ServiceYearID, e.ServiceYearName }), "ServiceYearID", "ServiceYearName");

            ViewBag.FastivalBonusPercentageDD = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "35", Text = "35 %" },
                    new SelectListItem { Value = "40", Text = "40 %" },
                    new SelectListItem { Value = "45", Text = "45 %" },
                    new SelectListItem { Value = "50", Text = "50 %" },
                    new SelectListItem { Value = "60", Text = "60 %" },
                    new SelectListItem { Value = "70", Text = "70 %" },
                    new SelectListItem { Value = "100", Text = "100 %" }
                }, "Value", "Text");

            ViewBag.BonusDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");


            ViewBag.PFEmployeeContributionDD = new SelectList(new[]
                {
                    new { Value = "5", Text = "5 %" },
                    new { Value = "6", Text = "6 %" },
                    new { Value = "7", Text = "7 %" },
                    new { Value = "8", Text = "8 %" },
                    new { Value = "9", Text = "9 %" },
                    new { Value = "10", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFOrgContributionDD = new SelectList(new[]
                {
                    new { Value = "5", Text = "5 %" },
                    new { Value = "6", Text = "6 %" },
                    new { Value = "7", Text = "7 %" },
                    new { Value = "8", Text = "8 %" },
                    new { Value = "9", Text = "9 %" },
                    new { Value = "10", Text = "10 %" }
                }, "Value", "Text");


            ViewBag.PFDependsOnDD = new SelectList(new[]
                {
                    new { Value = "Gross Salary", Text = "Gross Salary" },
                    new { Value = "Basic Salary", Text = "Basic Salary" }
                }, "Value", "Text");


            #endregion

            SetSmartPageCode(118000);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeBenifitData(int employeeId)
        {
            var employeeBenifitData = await _employeeBenifitService.GetEmployeeBenifitByEmployeeIdAsync(employeeId);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmployeeBenifitPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
                }

                // Save or update the employee benefits
                bool isSuccess = await _employeeBenifitService.SaveOrUpdateEmployeeBenefitsAsync(model);
                if (isSuccess)
                {
                    return Json(new { success = true, message = "Employee benefits saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save employee benefits." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = "An error occurred while saving employee benefits." });
            }
        }

    }
}