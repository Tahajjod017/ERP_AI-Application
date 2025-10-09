using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpAllowance;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.DevTools.V134.Page;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    [Authorize]
    public class PayRollEmployeesAllowanceController : BaseController
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes;
        private readonly IGenericRepository<Percentages> percentagesService;
        private readonly IPayRollEmpAllowanceService payRollEmpAllowanceService;
        private readonly IGenericRepository<CalculationTypes> calculationTypes;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoService;
        public PayRollEmployeesAllowanceController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes, IGenericRepository<Percentages> percentagesService, IPayRollEmpAllowanceService payRollEmpAllowanceService, IGenericRepository<CalculationTypes> calculationTypes, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoService) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.yearlyEndBonusTypes = yearlyEndBonusTypes;
            this.percentagesService = percentagesService;
            this.payRollEmpAllowanceService = payRollEmpAllowanceService;
            this.calculationTypes = calculationTypes;
            this.appDb = appDb;
            this.employeeOfficeInfoService = employeeOfficeInfoService;
        }

        public  async Task< IActionResult> Index()
        {
           
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
            var organizationId = await employeeOfficeInfoService.AllActive().Where(x => x.EmployeeID == employeeId).Select(x => x.OrganizationID).FirstOrDefaultAsync();
            var organizationList = organization.AllActive().ToList();
           
            ViewBag.OrganizationDD = new SelectList( organizationList,"OrganizationID", "OrganizationName",organizationId );
            ViewBag.SalaryTypesDD = new SelectList(salaryTypes.AllActive(), "SalaryTypeID", "SalaryTypeName");
            ViewBag.YearlyBonusTypeDD = new SelectList(yearlyEndBonusTypes.AllActive(), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");
            ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
            ViewBag.CalculationTypeDD = new SelectList(calculationTypes.AllActive(), "CalculationTypeID", "CalculationTypeName");
          

            return View();
        }


       
       


        #region Save Data

        [Route("PayRollEmployeesAllowance/SavePayRollEmpAlowance")]

        [HttpPost]

        public async Task<IActionResult> SavePayRollEmpAlowance([FromBody]PayRollEmpAllowanceSaveVM model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}
                var data = await payRollEmpAllowanceService.SavePayRollEmpAllowance(model);
                if (data.Success) 
                {
                    return Json(new { success =data.Success, message =data.Message });
                }
                return Json(new { success = data.Success, message = data.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });

            }
        }

        #endregion

    
        #region Get Allowance Type according to Organization
        [Route("PayRollEmployeesAllowance/SelectAllowanceTypeAsync")]
        [HttpGet]
        public async Task<IActionResult> SelectAsync(int id)
        {
            try
            {
                var data = await payRollEmpAllowanceService.SelectAsync(id);
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
