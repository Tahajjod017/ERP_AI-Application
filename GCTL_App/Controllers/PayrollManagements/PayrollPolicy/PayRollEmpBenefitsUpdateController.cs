using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class PayRollEmpBenefitsUpdateController : BaseController
    {

        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes;
        private readonly IGenericRepository<Percentages> percentagesService;
        private readonly IGenericRepository<CalculationTypes> calculationTypes;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoService;
        private readonly IEmployeeBenefitsService employeeBenefitsService;

        public PayRollEmpBenefitsUpdateController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes, IGenericRepository<Percentages> percentagesService, IGenericRepository<CalculationTypes> calculationTypes, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoService, IEmployeeBenefitsService employeeBenefitsService) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.yearlyEndBonusTypes = yearlyEndBonusTypes;
            this.percentagesService = percentagesService;
            this.calculationTypes = calculationTypes;
            this.appDb = appDb;
            this.employeeOfficeInfoService = employeeOfficeInfoService;
            this.employeeBenefitsService = employeeBenefitsService;
        }

        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
            var organizationId = await employeeOfficeInfoService.AllActive().Where(x => x.EmployeeID == employeeId).Select(x => x.OrganizationID).FirstOrDefaultAsync();
            var organizationList = organization.AllActive().ToList();
            if (organizationList.Count == 1)
            {
                ViewBag.OrganizationDD = new SelectList(organizationList, "OrganizationID", "OrganizationName", organizationList.First().OrganizationID);
            }
            else
            {
                ViewBag.OrganizationDD = new SelectList(organizationList, "OrganizationID", "OrganizationName", organizationId);
            }
            ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
            ViewBag.CalculationTypeDD = new SelectList(calculationTypes.AllActive(), "CalculationTypeID", "CalculationTypeName");
            return View();
        }

        #region Get Allowance Type according to Organization
        [Route("PayRollEmpBenefitsUpdate/SelectAllowanceTypeAsync")]
        [HttpGet]
        public async Task<IActionResult> SelectAsync(int id)
        {
            try
            {
                var data = await employeeBenefitsService.SelectAsync(id);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get Allowance Type according to Organization
        [Route("PayRollEmpBenefitsUpdate/Save")]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody]EmployeeBenefitsVM model)
        {
            try
            {
                var data = await employeeBenefitsService.SaveEmployeeBenefitsAsync(model);
                return Json(new {Success=data.Success, Message=data.Message});
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

    }
}
