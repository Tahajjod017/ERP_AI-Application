using GCTL.Service.Employees.EmployeeReport;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeReportController : BaseController
    {
        private readonly IEmployeeReportService _employeeReportService;
        public EmployeeReportController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeReportService employeeReportService) : base(translateService, userProfileService)
        {
            _employeeReportService = employeeReportService;
        }


        //[Route("EmployeeReports/GenerateIndiEmpDetailsPDF")]
        [HttpPost]
        public async Task<IActionResult> GenerateIndiEmpDetailsPDF(int id)
        {
            var pdfBytes = await _employeeReportService.GenaratePDF(id);
            return File(pdfBytes, "application/pdf", $"Employee_{id}.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAllEmployeeExcel()
        {
            try
            {
                var excelBytes = await _employeeReportService.GenerateEmployeeExcelReportAsync();
                var fileName = $"Employee_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return StatusCode(500, "Error generating Excel report: " + ex.Message);
            }
        }
    }
}
