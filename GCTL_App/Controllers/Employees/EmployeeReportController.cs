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
    }
}
