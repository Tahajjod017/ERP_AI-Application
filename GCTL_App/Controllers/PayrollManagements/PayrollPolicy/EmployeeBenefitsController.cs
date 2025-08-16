using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using GCTL.Service.PayRollManagements.PayRollPolicy;
using System.Threading.Tasks.Sources;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using System.Security.Claims;
using GCTL.Core.Helpers;
using Microsoft.Identity.Client;
using GCTL_App.ViewModels.PayRollManagements.PayRollPolicy;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class EmployeeBenefitsController : BaseController
    {
        private readonly IGenericRepository<Organization> organization;
        private readonly IGenericRepository<SalaryTypes> salaryTypes;
        private readonly IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes;
        private IEmployeeBenefitsService employeeBenefitsService;
        private readonly IGenericRepository<Percentages> percentagesService;
        public EmployeeBenefitsController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organization, IGenericRepository<SalaryTypes> salaryTypes, IEmployeeBenefitsService employeeBenefitsService, IGenericRepository<YearlyEndBonusTypes> yearlyEndBonusTypes, IGenericRepository<Percentages> percentagesService) : base(translateService, userProfileService)
        {
            this.organization = organization;
            this.salaryTypes = salaryTypes;
            this.employeeBenefitsService = employeeBenefitsService;
            this.yearlyEndBonusTypes = yearlyEndBonusTypes;
            this.percentagesService = percentagesService;
        }

        public async Task<IActionResult> Index()
        {
            PayRollEmpBenefitsPageVM model = new PayRollEmpBenefitsPageVM();
            
            ViewBag.OrganizationDD = new SelectList( organization.AllActive(), "OrganizationID", "OrganizationName");
            ViewBag.SalaryTypesDD = new SelectList(salaryTypes.AllActive(), "SalaryTypeID", "SalaryTypeName");
            ViewBag.YearlyBonusTypeDD=new SelectList(yearlyEndBonusTypes.AllActive(), "YearlyEndBonusTypeID", "YearlyEndBonusTypeName");
            ViewBag.PercenatageDD = new SelectList(percentagesService.AllActive(), "PercentageValue", "PercentageValue");
            return View(model);
        }
        #region Save Data


        #endregion]
        
        #region Get All Data List

        [Route("EmployeeBenefits/GetAllTableListAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllTableListAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", int? organizationId = null)
        {
            try
            {
              
                var data = await employeeBenefitsService.GetAllTableAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder, organizationId);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        #endregion
        #region Save    update  Datum
        [Route("EmployeeBenefits/Create")]
        [HttpPost]
        public async Task<IActionResult> Create(PayRollEmpBenefitsSaveVM entityVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                    return Json(new { Success = false, message = string.Join(", ", errors) });
                }
                var data = await employeeBenefitsService.SaveEmployeeBenefits(entityVM);
                    return Json(data);

                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }

        [Route("EmployeeBenefits/Update")]
        [HttpPost]
        public async Task<IActionResult> Update(PayRollEmpBenefitsUpdate entityVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors) .Select(e => e.ErrorMessage).ToList();
                    return Json(new { Success = false, message = string.Join(", ", errors) });
                }
                var data = await employeeBenefitsService.UpdateEmployeeBenefits(entityVM);
                return Json(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }

        #endregion



        #region Get By Id
        [Route("EmployeeBenefits/GetByIdEmpBenefits")]
        [HttpGet]
        public async Task<IActionResult> GetByIdEmpBenefits(int employeeBenefitID)
        {
            try
            {
                var data = await employeeBenefitsService.GetById(employeeBenefitID);

                if (data == null)
                {
                    return NotFound(new { message = "Employee benefit not found." });
                }

                return Json(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the data.", error = ex.Message });
            }
        }

        #endregion

        #region Delete Leave Request
        [Route("EmployeeBenefits/SoftDeletePayRollEmpRequest")]
        [HttpPost]
        public async Task<IActionResult> SoftDeletePayRollEmpRequest(DeleteRequestVM deleteRequestVM)
        {
            try
            {
                var data = await employeeBenefitsService.SoftDeletePayRollEmpRequest(deleteRequestVM);
                return Json(data);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { message = ex.Message });

            }
        }
        #endregion
    }
}
