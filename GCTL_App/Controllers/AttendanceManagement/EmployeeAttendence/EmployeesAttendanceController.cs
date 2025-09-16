using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.AttendanceManagement.EmployeeAttendence;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace GCTL_App.Controllers.AttendanceManagement.EmployeeAttendence
{
    public class EmployeesAttendanceController : BaseController
    {
        private readonly IEmployeeAttendanceReport _employeeAttendanceReport;
        private readonly ILocalizationContext _loc;
        private readonly IGenericRepository<Statuses> _organizationRepository;
        public EmployeesAttendanceController(ITranslateService translateService, IUserProfileService userProfileService, IEmployeeAttendanceReport employeeAttendanceReport, ILocalizationContext loc, IGenericRepository<Statuses> organizationRepository) : base(translateService, userProfileService)
        {
            _employeeAttendanceReport = employeeAttendanceReport;
            _loc = loc;
            _organizationRepository = organizationRepository;
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
           

            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();
            int? orgId = await GetCurrentOrganizationIdAsync();
            //var orgId = await GetCurrentOrganizationIdAsync();

            if (currentEmployeeId.HasValue)
            {
                var getEmployeeTotalHoursRelated = await _employeeAttendanceReport.GetAttendanceDetailsAsync(currentEmployeeId.Value);
                var getEmployeeTotalHoursRelated2 = await _employeeAttendanceReport.GetAttendanceProgressBarAsync(currentEmployeeId.Value);
                var getEmployeeFirstPunch = await _employeeAttendanceReport.GetEmployeeFirstPunchInTimeAsync(currentEmployeeId.Value);
                var getEmployeeDetails = await _employeeAttendanceReport.GetTotalHoursForWeek(currentEmployeeId.Value, orgId.Value, null);
                var getEmployeeDetailsMonth = await _employeeAttendanceReport.GetTotalHoursForMonth(currentEmployeeId.Value, orgId.Value, null);

                ViewData["TotalHoursWeek"] = getEmployeeDetails.ToString("F2");
                ViewData["TotalHoursMonth"] = getEmployeeDetailsMonth.ToString("F2");

                ViewData["ProductionTime"] = getEmployeeTotalHoursRelated2.TotalWorkingHours;
                ViewData["ProductionTimeMinute"] = getEmployeeTotalHoursRelated.ProductionTimeMinute;
               // ViewData["CheckInTime"] = getEmployeeTotalHoursRelated.CheckInTime; 
                ViewData["CheckInTime"] = getEmployeeFirstPunch; 
                //ViewBag.ProductionTime = getEmployeeTotalHoursRelated.ProductionTime;
                ViewData["Overtime"] = getEmployeeTotalHoursRelated.Overtime;
                ViewData["TotalWorkingHours"] = getEmployeeTotalHoursRelated.TotalWorkingHours;
                //ViewData["CheckInTime"] = getEmployeeTotalHoursRelated.CheckInTime;
            }
            else
            {
                // Handle the case where currentEmployeeId is null if necessary  
            }
            //var serverTime = DateTime.Now.ToString("hh:mm tt, dd MMM yyyy");
            // Pass the current time to the view  
            ViewData["CurrentTime"] = DateTimeExtensions.NowDateTime(_loc);
           // ViewData["TimeZone"] = _loc.Zone.Id;  // Time zone id
            //ViewData["Locale"] = _loc.DatePattern;  // Locale for formatting the date

            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive().Where(x=>x.StatusType== "Present/Absent"), "StatusID", "StatusName");

            return View();
        }
        public async Task<IActionResult> GetCurrentTimeAsync()
        {
            // Simulating an async operation (e.g., fetching data from a database or external service)
            await Task.Delay(100); // Simulate some delay to make the method async.

            // Fetch the current time (using your existing logic)
            var currentTime = DateTimeExtensions.NowDateTime(_loc);

            // Return the time as a JsonResult
            return Json(currentTime);
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceProgressBar()
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();
            int empId = 0;
            // Fix for CS1503: Ensure the nullable int is converted to a non-nullable int before passing it to the method  
            if (!currentEmployeeId.HasValue)
            {
                // You can log this or throw an exception depending on how critical this is
                return Json("Current employee ID could not be determined.");
            }
            empId = currentEmployeeId.Value;


            var attendanceData = await _employeeAttendanceReport.GetAttendanceProgressBarAsync(empId);
            return Json(attendanceData);
 

        }

        #region table
        public async Task<IActionResult> GetAlls(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "HolidayTitle", string sortOrder = "desc", int? organizationID = null, int? employeeId = null, int? statusID = null, string? sortId = "")
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var result = await _employeeAttendanceReport.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, organizationID, currentEmployeeId, statusID ,sortId);
            return Json(result);

        }

        #endregion
        public async Task<IActionResult> GetEmployeeAttendanceData(int userId)
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            // Fix for CS1503: Ensure the nullable int is converted to a non-nullable int before passing it to the method  
            if (currentEmployeeId.HasValue)
            {
                var attendanceData = await _employeeAttendanceReport.GetAttendanceDetailsAsync(currentEmployeeId.Value);
                return Json(attendanceData);
            }
            else
            {
                // Handle the case where currentEmployeeId is null  
                return Json(null);
            }
        }
        public async Task<IActionResult> GetEmployeeAttendanceData2(int userId)
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            // Fix for CS1503: Ensure the nullable int is converted to a non-nullable int before passing it to the method  
            if (currentEmployeeId.HasValue)
            {
                var attendanceData = await _employeeAttendanceReport.CalculateWorkingHours(currentEmployeeId.Value);
                return Json(attendanceData);
            }
            else
            {
                // Handle the case where currentEmployeeId is null  
                return Json(null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeAttendanceActivity()
        {
            int? currentEmployeeId = await GetCurrentEmployeeIdAsync();

            if (!currentEmployeeId.HasValue)
            {
                return Json(null);
            }

            // Use the interface method
            var attendanceData = await _employeeAttendanceReport
                .GetEmployeePunchActivityAsync(currentEmployeeId.Value);

            return Json(attendanceData);
        }


    }
}
