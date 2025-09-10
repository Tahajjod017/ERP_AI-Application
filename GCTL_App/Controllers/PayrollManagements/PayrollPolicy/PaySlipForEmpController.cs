using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    public class PaySlipForEmpController : BaseController
    {
        private readonly IPayRollEmpSalaryService payRollEmpSalaryService;
        private readonly AppDbContext appDb;
        public PaySlipForEmpController(ITranslateService translateService, IUserProfileService userProfileService, IPayRollEmpSalaryService payRollEmpSalaryService, AppDbContext appDb) : base(translateService, userProfileService)
        {
            this.payRollEmpSalaryService = payRollEmpSalaryService;
            this.appDb = appDb;
        }

        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
            ViewBag.EmployeeId = employeeId;    
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
