using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.EmployeeAttendence;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GCTL_App.Controllers.AttendanceManagement.EmployeeAttendence
{
    public class EmployeesAttendanceController : BaseController
    {
        private readonly IEmployeeAttendanceReport _employeeAttendanceReport;
        public EmployeesAttendanceController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeAttendanceReport employeeAttendanceReport) : base(translateService, userProfileService)
        {
            _employeeAttendanceReport = employeeAttendanceReport;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }
            // user profile  
            SetUserProfile();
            // Get the current time from the server  
            var serverTime = DateTime.Now.ToString("hh:mm tt, dd MMM yyyy");

            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            if (currentEmployeeId.HasValue)
            {
                var getEmployeeTotalHoursRelated = await _employeeAttendanceReport.GetAttendanceDetailsAsync(currentEmployeeId.Value);
                var getEmployeeDetails = await _employeeAttendanceReport.GetTotalHoursForWeek(currentEmployeeId.Value,2,null);

                ViewData["TotalHoursWeek"] = getEmployeeDetails.ToString("F2");


                ViewData["ProductionTime"] = getEmployeeTotalHoursRelated.ProductionTime;
                ViewData["CheckInTime"] = getEmployeeTotalHoursRelated.CheckInTime; 
                //ViewBag.ProductionTime = getEmployeeTotalHoursRelated.ProductionTime;
                ViewData["Overtime"] = getEmployeeTotalHoursRelated.Overtime;
                ViewData["TotalWorkingHours"] = getEmployeeTotalHoursRelated.TotalWorkingHours;
                ViewData["CheckInTime"] = getEmployeeTotalHoursRelated.CheckInTime;
            }
            else
            {
                // Handle the case where currentEmployeeId is null if necessary  
            }
            
            // Pass the current time to the view  
            ViewData["CurrentTime"] = serverTime;

            return View();
        }
        #region table
        public async Task<IActionResult> GetAlls(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayTitle", string sortOrder = "desc", int? organizationID = null, int? employeeId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var result = await _employeeAttendanceReport.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID, currentEmployeeId);
            return Json(result);

        }
        #endregion
    }
}
