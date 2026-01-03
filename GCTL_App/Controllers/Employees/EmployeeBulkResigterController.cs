using GCTL.Core.ServiceExtensions;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeBulkRegister;
using GCTL.Service.Employees.EmployeeBulkRegister;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace GCTL_App.Controllers.Employees
{
    public class EmployeeBulkResigterController : BaseController
    {
        private readonly IEmployeeBulkRegisterService _employeeBulkRegisterService;

        public EmployeeBulkResigterController(
            ITranslateService translateService,
            IUserProfileService userProfileService,
            IEmployeeBulkRegisterService employeeBulkRegisterService)
            : base(translateService, userProfileService)
        {
            _employeeBulkRegisterService = employeeBulkRegisterService;
        }

        public IActionResult Index()
        {
            // Clear session when returning to index
            HttpContext.Session.Remove("ExcelEmployees");

            EmpPagedResult<EmployeeExcelVM> emp = new EmpPagedResult<EmployeeExcelVM>
            {
                Data = new List<EmployeeExcelVM>(),
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 0
            };
            return View(emp);
        }

        // 📥 Download Template
        public IActionResult DownloadTemplate()
        {
            var file = _employeeBulkRegisterService.GenerateExcelTemplate();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Employee_Upload_Template.xlsx");
        }

        // 📤 Upload Excel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {
                var employees = ReadExcel(excelFile);

                // Remove empty entries
                employees = employees.Where(e =>
                    !string.IsNullOrWhiteSpace(e.EmployeeCode) ||
                    !string.IsNullOrWhiteSpace(e.FirstName) ||
                    !string.IsNullOrWhiteSpace(e.Email))
                    .ToList();

                if (!employees.Any())
                {
                    TempData["Error"] = "The Excel file contains no valid data.";
                    return RedirectToAction("Index");
                }

                HttpContext.Session.SetObject("ExcelEmployees", employees);
                TempData["Success"] = $"Successfully loaded {employees.Count} employees from Excel.";

                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error reading Excel file: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // 👀 Preview with Search and Pagination
        public IActionResult Preview(int page = 1, int pageSize = 10, string search = "")
        {
            var model = _employeeBulkRegisterService.GetPreviewData(page, pageSize, search);

            // Check for duplicates
            var allData = HttpContext.Session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees");
            if (allData != null && allData.Any())
            {
                var duplicateCheck = _employeeBulkRegisterService.CheckDuplicates(allData);

                ViewBag.DuplicateEmployeeCodes = duplicateCheck.DuplicateEmployeeCodes;
                ViewBag.DuplicateEmails = duplicateCheck.DuplicateEmails;
                ViewBag.HasDuplicates = duplicateCheck.HasDuplicates;

                if (duplicateCheck.HasDuplicates)
                {
                    var warnings = new List<string>();

                    if (duplicateCheck.DuplicateEmployeeCodes.Any())
                    {
                        warnings.Add($"<strong>Employee Codes:</strong> {string.Join(", ", duplicateCheck.DuplicateEmployeeCodes.Take(5))}");
                        if (duplicateCheck.DuplicateEmployeeCodes.Count > 5)
                            warnings.Add($"... and {duplicateCheck.DuplicateEmployeeCodes.Count - 5} more");
                    }

                    if (duplicateCheck.DuplicateEmails.Any())
                    {
                        warnings.Add($"<strong>Emails:</strong> {string.Join(", ", duplicateCheck.DuplicateEmails.Take(5))}");
                        if (duplicateCheck.DuplicateEmails.Count > 5)
                            warnings.Add($"... and {duplicateCheck.DuplicateEmails.Count - 5} more");
                    }

                    TempData["Warnings"] = string.Join("<br>", warnings);
                }
            }

            // Pass search term to view
            ViewBag.SearchTerm = search;
            ViewBag.PageSize = pageSize;

            return View("Index", model);
        }

        // Get Employee for Edit
        [HttpGet]
        public IActionResult GetEmployee(int index)
        {
            var data = HttpContext.Session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees");

            if (data == null || index < 0 || index >= data.Count)
                return Json(new { success = false, message = "Employee not found." });

            return Json(new { success = true, employee = data[index] });
        }

        // Update Employee in Session
        [HttpPost]
        public IActionResult UpdateEmployee([FromBody] EmployeeUpdateRequest request)
        {
            var data = HttpContext.Session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees");

            if (data == null || request.Index < 0 || request.Index >= data.Count)
                return Json(new { success = false, message = "Employee not found." });

            try
            {
                data[request.Index] = new EmployeeExcelVM
                {
                    EmployeeCode = request.EmployeeCode?.Trim(),
                    FirstName = request.FirstName?.Trim(),
                    LastName = request.LastName?.Trim(),
                    Gender = request.Gender?.Trim(),
                    Email = request.Email?.Trim(),
                    OfficialEmail = request.OfficialEmail?.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    Branch = request.Branch?.Trim(),
                    JoiningDate = !string.IsNullOrEmpty(request.JoiningDate)
                        ? DateTime.Parse(request.JoiningDate)
                        : (DateTime?)null,
                    Comment = request.Comment?.Trim(),
                    Designation = request.Designation?.Trim(),
                    DepartmentName = request.DepartmentName?.Trim(),
                    ImmediateSupervisorName = request.ImmediateSupervisorName?.Trim(),
                    DepartmentHeadName = request.DepartmentHeadName?.Trim()
                };

                HttpContext.Session.SetObject("ExcelEmployees", data);
                return Json(new { success = true, message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating employee: {ex.Message}" });
            }
        }

        // Delete Employee from Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEmployee(int index)
        {
            var data = HttpContext.Session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees");

            if (data == null || index < 0 || index >= data.Count)
                return Json(new { success = false, message = "Employee not found." });

            try
            {
                data.RemoveAt(index);
                HttpContext.Session.SetObject("ExcelEmployees", data);
                return Json(new { success = true, message = "Employee removed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error removing employee: {ex.Message}" });
            }
        }

        // ✅ Confirm Save
        [HttpPost]
        public async Task<IActionResult> ConfirmSave(BaseViewModel? baseView)
        {
            var data = HttpContext.Session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees");

            if (data == null || !data.Any())
                return Json(new { success = false, message = "No data to save." });

            try
            {
                var orgaId = await GetCurrentOrganizationIdAsync();

                var result = await _employeeBulkRegisterService.SaveEmployeeDataAsync(data, baseView, orgaId);

                if (result.Success)
                {
                    HttpContext.Session.Remove("ExcelEmployees");
                    return Json(new
                    {
                        success = true,
                        message = result.Message
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving data: {ex.Message}" });
            }
        }

        // 🔹 Excel Reader
        private List<EmployeeExcelVM> ReadExcel(IFormFile file)
        {
            var list = new List<EmployeeExcelVM>();

            using var stream = new MemoryStream();
            file.CopyTo(stream);

            using var package = new ExcelPackage(stream);

            if (package.Workbook.Worksheets.Count == 0)
                throw new Exception("Excel file has no worksheets.");

            var sheet = package.Workbook.Worksheets[0];

            if (sheet.Dimension == null)
                throw new Exception("Excel worksheet is empty.");

            int rows = sheet.Dimension.Rows;

            for (int r = 2; r <= rows; r++)
            {
                list.Add(new EmployeeExcelVM
                {
                    EmployeeCode = sheet.Cells[r, 1].Text?.Trim(),
                    FirstName = sheet.Cells[r, 2].Text?.Trim(),
                    LastName = sheet.Cells[r, 3].Text?.Trim(),
                    Gender = sheet.Cells[r, 4].Text?.Trim(),
                    Email = sheet.Cells[r, 5].Text?.Trim(),
                    OfficialEmail = sheet.Cells[r, 6].Text?.Trim(),
                    PhoneNumber = sheet.Cells[r, 7].Text?.Trim(),
                    Branch = sheet.Cells[r, 8].Text?.Trim(),
                    JoiningDate = DateTime.TryParse(sheet.Cells[r, 9].Text, out var jd) ? jd : null,
                    Comment = sheet.Cells[r, 10].Text?.Trim(),
                    Designation = sheet.Cells[r, 11].Text?.Trim(),
                    DepartmentName = sheet.Cells[r, 12].Text?.Trim(),
                    ImmediateSupervisorName = sheet.Cells[r, 13].Text?.Trim(),
                    DepartmentHeadName = sheet.Cells[r, 14].Text?.Trim()
                });
            }

            return list;
        }
    }




}
