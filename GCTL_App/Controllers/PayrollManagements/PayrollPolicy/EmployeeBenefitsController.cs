using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using System.Threading.Tasks.Sources;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class EmployeeBenefitsController : BaseController
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes;
        private IEmployeeBenefitsService employeeBenefitsService;
        public EmployeeBenefitsController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IEmployeeBenefitsService employeeBenefitsService, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.employeeBenefitsService = employeeBenefitsService;
            this.yearlyEndBonusTypes = yearlyEndBonusTypes;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrganizationDD = new SelectList( organization.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.SalaryTypesDD = new SelectList(salaryTypes.AllActive(), "SalaryTypeID", "SalaryTypeName");
            ViewBag.YearlyBonusTypeDD=new SelectList(yearlyEndBonusTypes.AllActive(), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");
            return View();
        }
        [Route("EmployeeBenefits/Create")]
        [HttpPost]
        public async Task<IActionResult> Create(PayRollEmpBenefitsSaveVM entityVM)
        {
            try
            {
                var data = await employeeBenefitsService.SaveEmployeeBenefits(entityVM);
               return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
