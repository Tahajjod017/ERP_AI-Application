using GCTL.Core.ViewModels.Employee.EmployeeListVM;
using GCTL.Service.Employees.EmployeeReport;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.Employees
{
    [Authorize]
    public class EmployeeReportController : BaseController
    {
        private readonly IEmployeeReportService _employeeReportService;
        public EmployeeReportController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeReportService employeeReportService) : base(translateService, userProfileService)
        {
            _employeeReportService = employeeReportService;
        }


        
        [HttpPost]
        public async Task<IActionResult> GenerateIndiEmpDetailsPDF(int id)
        {
            var pdfBytes = await _employeeReportService.GenaratePDF(id);
            return File(pdfBytes, "application/pdf", $"Employee_{id}.pdf");
        }

       


        [HttpPost]
        public async Task<IActionResult> GenerateAllEmployeeExcel( EmployeeFilterModel filters)
        {
            try
            {
                var excelBytes = await _employeeReportService.GenerateEmployeeExcelReportAsync(filters);
                var fileName = $"Employee_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                // return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error generating Excel report: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> GenerateEmployeePdfPreview(EmployeeFilterModel filters)
        {
            try
            {
                var pdfBytes = await _employeeReportService.GenerateEmployeePdfPreviewAsync(filters);
                var fileName = $"Employee_Preview_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error generating PDF preview: " + ex.Message);
            }
        }


    }
}
