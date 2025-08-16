using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.PayRollManagements.PayRollPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class PayRollEmployeesAllowanceController : BaseController
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes;
        private readonly IGenericRepository<Percentages> percentagesService;
        private readonly IPayRollEmpAllowanceService payRollEmpAllowanceService;
        public PayRollEmployeesAllowanceController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes, IGenericRepository<Percentages> percentagesService, IPayRollEmpAllowanceService payRollEmpAllowanceService) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.yearlyEndBonusTypes = yearlyEndBonusTypes;
            this.percentagesService = percentagesService;
            this.payRollEmpAllowanceService = payRollEmpAllowanceService;
        }

        public IActionResult Index()
        {
            PayRollEmpAllowancePageVM model =new PayRollEmpAllowancePageVM();
            ViewBag.OrganizationDD = new SelectList(organization.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.SalaryTypesDD = new SelectList(salaryTypes.AllActive(), "SalaryTypeID", "SalaryTypeName");
            ViewBag.YearlyBonusTypeDD = new SelectList(yearlyEndBonusTypes.AllActive(), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");
            ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
            return View(model);
        }
        #region Save Data
        [Route("PayRollEmployeesAllowance/SavePayRollEmpAlowance")]
        public async Task<IActionResult> SavePayRollEmpAlowance(PayRollEmpAllowanceSaveVM model)
        {
            try
            {
               var data=await payRollEmpAllowanceService.SavePayRollEmpAllowance(model);
                return Json(data);  
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
