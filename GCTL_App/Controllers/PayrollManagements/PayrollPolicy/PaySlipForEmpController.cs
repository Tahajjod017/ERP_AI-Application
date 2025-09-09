using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class PaySlipForEmpController : BaseController
    {
        private readonly IPayRollEmpSalaryService payRollEmpSalaryService;
        public PaySlipForEmpController(ITranslateService translateService, IUserProfileService userProfileService, IPayRollEmpSalaryService payRollEmpSalaryService) : base(translateService, userProfileService)
        {
            this.payRollEmpSalaryService = payRollEmpSalaryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("PaySlipForEmp/GetPaySlip")]
        [HttpGet]
        public async Task<IActionResult> GetPaySlip(int id)
        {
            try
            {
                var data =await payRollEmpSalaryService.GetPaySlip(id);
                return Json(new {Success=data.Success, Data=data.Data});
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
