using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpSalary;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.PayRollEmpSalary;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace GCTL_App.Controllers.PayrollManagements.PayrollPolicy
{
    [Authorize]
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
        #region Pay slop Get 
        [Route("PaySlipForEmp/GetPaySlip")]
        [HttpGet]
        public async Task<IActionResult> GetPaySlip(int id)
        {
            try
            {
                var data =await payRollEmpSalaryService.GetPaySlip(id);
                return Json(new {Success=data.Success, Data=data.Data, Message = data.Message});
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Save Pay slip
        //[Route("PaySlipForEmp/Save")]
        //[HttpPost]
        //public async Task<IActionResult> Save([FromBody]PayRollEmpSalarySaveVM model) 
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            // Collect all validation errors
        //            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        //            return Json(new
        //            {
        //                Success = false,Message =errors, 
        //            });
        //        }
        //        var data=await payRollEmpSalaryService.SaveAsync(model);
        //        return Json(new { Success = data.Success, Message = data.Message});
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        [Route("PaySlipForEmp/SaveAndPdf")]
        [HttpPost]
        public async Task<IActionResult> SaveAndGeneratePDF([FromBody] PayRollEmpSalarySaveVM model)
        {
            // 1️⃣ Save payslip
            var saveResult = await payRollEmpSalaryService.SaveAsync(model);

            if (!saveResult.Success || saveResult.Data == null)
                return Json(new { Success = false, Message = saveResult.Message });

            // 2️⃣ Get Payslip ID
            int payslipId = ((PaySlips)saveResult.Data).PaySlipID;

            // 3️⃣ Generate PDF
            var pdfBytes = await payRollEmpSalaryService.GeneratePdf(payslipId);

            return File(pdfBytes, "application/pdf", $"PaySlip_{payslipId}.pdf");
        }

        #endregion

        #region Pay slip
        [Route("PaySlipForEmp/Pdf")]
        [HttpPost]
        public async Task<IActionResult> GenerateIndiEmpDetailsPDF(int id)
        {
            var pdfBytes = await payRollEmpSalaryService.GeneratePdf(id);
            return File(pdfBytes, "application/pdf", $"Pay Slip{id}.pdf");
        }
        #endregion


    }
}
