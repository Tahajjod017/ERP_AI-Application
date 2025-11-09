using Dapper;
using GCTL.Core.Enums;
using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using static GCTL.Service.AdminSettings.GeneralSettings.UtcTimeHelper;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports
{
    public class MonthlyReportService:AppService<Attendance>, IMonthlyReportService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        private readonly LeaveHelper _leaveHelper;
        private readonly IGenericRepository<EmployeeOfficeInfo> _genericEmployeeOfficeInfo;
        private readonly ILocalizationContext _localizationContext;

        public MonthlyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, HolidayHelper holidayHelper, WeekendHelper weekendHelper, LeaveHelper leaveHelper, IGenericRepository<EmployeeOfficeInfo> genericEmployeeOfficeInfo, ILocalizationContext localizationContext) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _leaveHelper = leaveHelper;
            _genericEmployeeOfficeInfo = genericEmployeeOfficeInfo;
            _localizationContext = localizationContext;
        }

        //public async Task<SelectListItem> GetMonthAsync()
        //{
        //    // Get all months from the enum
        //    var months = Enum.GetValues(typeof(Month))
        //                     .Cast<Month>()
        //                     .Select(m => new SelectListItem
        //                     {
        //                         Text = m.ToString(), // Month name (e.g., January)
        //                         Value = ((int)m).ToString() // Month number (e.g., 1 for January)
        //                     })
        //                     .ToList();

        //    return months;
        //}
        public async Task<PaginationService<Attendance, EmployeeAttendanceMonthlyVM>.PaginationResult<EmployeeAttendanceMonthlyVM>>
           GetFilteredPaginatedEmployeeAttendanceAsync(
                                                  int organizationID,
                                                  int departmentID,
                                                  int month,
                                                  int year,
                                                  int pageNumber = 1,
                                                  int pageSize = 5,
                                                  string sortColumn = "EmployeeName",
                                                  string sortOrder = "asc")
        {
            using (var connection = new SqlConnection("_connectionString"))
            {
                var parameters = new
                {
                    OrganizationID = organizationID,
                    DepartmentID = departmentID,
                    Month = month,
                    Year = year,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder
                };

                // Execute the stored procedure to get filtered attendance data
                var result = await connection.QueryAsync<EmployeeAttendanceMonthlyVM>(
                    "GetFilteredPaginatedEmployeeAttendance",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Execute the stored procedure to get the total count for pagination
                var totalCount = await connection.QuerySingleAsync<int>(
                    "GetFilteredPaginatedEmployeeAttendance",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var paginationResult = new PaginationService<Attendance, EmployeeAttendanceMonthlyVM>.PaginationResult<EmployeeAttendanceMonthlyVM>
                {
                    Data = result.ToList(),
                    //Pagination = new PaginationMetadata
                    //{
                    //    TotalItems = totalCount,
                    //    CurrentPage = pageNumber,
                    //    PageSize = pageSize,
                    //    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    //}
                };

                return paginationResult;
            }
        }

        public async Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetMonthlyAttendanceReport(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                              string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, List<int>? departmentIds = null, List<int>? employeeIds = null, string? monthyear = null, int? employeeId = null)
         {

            var query = _genericRepository.All()
                         .AsNoTracking()
                         .Include(x => x.Employee)
                         .Include(x => x.Employee.EmployeeOfficeInfoCreatedByNavigation)
                         .Include(x => x.Status)
                         .Include(x => x.Shift)
                         .Where(x => x.DeletedAt == null && x.EmployeeID == employeeId); // Filter by EmployeeID

            var employeeDepartments = await _genericEmployeeOfficeInfo.All()
                                         .Include(e => e.Department) // Ensure the Department is loaded
                                         .Where(e => e.DeletedAt == null)
                                         .Select(e => new
                                         {
                                             EmployeeID = e.EmployeeID,
                                             DepartmentName = e.Department != null && e.Department.DepartmentName != null
                                                              ? e.Department.DepartmentName
                                                              : "-"
                                         })
                                         .ToListAsync();


            var result = await PaginationService<Attendance, AttendanceEmployeeReportVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%"),

                x => new AttendanceEmployeeReportVM
                {
                    AttendanceID = x.AttendanceID,

                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    JobTitle = employeeDepartments.FirstOrDefault(e => e.EmployeeID == x.EmployeeID)?.DepartmentName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(x.CheckInTime.Value, DateTimeKind.Utc), _localizationContext) : "-",
                    CheckOutTime = x.CheckOutTime.HasValue ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(x.CheckOutTime.Value, DateTimeKind.Utc), _localizationContext)  // Convert UTC to local
                                    : "-",
                    //LateHour = x.LateTimeMinutes.HasValue ? x.LateTimeMinutes.Value.ToString("F2") : "-",
                    LateHour = FormatTime(x.LateTimeMinutes),
                    //EarlyHour = x.EarlyTimeMinutes.HasValue ? x.EarlyTimeMinutes.Value.ToString("F2") : "-",
                    EarlyHour = FormatTime(x.EarlyTimeMinutes),
                    RegularHour = FormatTime(x.OfficeTimeMinutes),
                    OvertimeHour = FormatTime(x.OvertimeMinutes),
                    WorkingHours = FormatTime(x.WorkingTimeMinutes),
                    Break = FormatTime(x.BreakTimeMinutes),

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }
        private string FormatTime(int? minutes)
        {
            if (!minutes.HasValue)
                return "-";

            int hours = minutes.Value / 60;
            int remainingMinutes = minutes.Value % 60;

            return $"{hours:D2}:{remainingMinutes:D2}"; // Formats as "HH:mm"
        }
        // Helper method to get the special day label
        private string GetSpecialDayLabel(int day, bool isLeaveDay, bool isWeekend, bool isHoliday)
        {
            if (isLeaveDay)
            {
                return "Leave";
            }
            if (isWeekend)
            {
                return "Weekend";
            }
            if (isHoliday)
            {
                return "Holiday";
            }
            return "Absent";
        }

        //public async Task<IActionResult> GetYearlyAttendanceReport(int? departmentId, int? organizationId, int? employeeId, string year)
        //{

        //    var startDate = DateOnly.ParseExact(year + "-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture); // Start of the year
        //    var endDate = startDate.AddYears(1).AddDays(-1);  // End of the year (December 31)

        //    // Get the weekend days for the organization and branch
        //    var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId.Value, null); // Pass the branch ID if needed

        //    // Get active holidays within the given date range
        //    var holidays = _holidayHelper.GetActiveHolidays(organizationId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

        //    // Query the Attendance and Shifts tables with the filters
        //    var query = _genericRepository.All()
        //        .AsNoTracking()  // Disable change tracking for better performance since we are only reading data
        //        .Include(x => x.Employee)  // Include the related Employee data
        //        .Include(x => x.Status)    // Include the related Status data
        //        .Include(x => x.Shift)     // Include the related Shift data
        //        .Where(x => x.DeletedAt == null &&  // Filter for deleted records
        //                   (employeeId == null || x.EmployeeID == employeeId) &&
        //                   (organizationId == null || x.Shift.OrganizationID == organizationId) &&
        //                   //(departmentId == null || x.Employee.DepartmentID == departmentId) &&
        //                   x.AttendanceDate >= startDate && x.AttendanceDate <= endDate)
        //        .Select(x => new
        //        {
        //            x.AttendanceID,
        //            x.EmployeeID,
        //            x.AttendanceDate,
        //            x.StatusID,
        //            x.ShiftID,
        //            x.CheckInTime,
        //            x.CheckOutTime,
        //            x.Remarks,
        //            x.LIP,
        //            x.LMAC,
        //            x.RegularHour,
        //            x.OvertimeHour,
        //            x.LateHour,
        //            x.EarlyHour,
        //            x.WorkingHour,
        //            ShiftName = x.Shift.ShiftName,  // Access ShiftName from the Shift navigation property
        //            OrganizationID = x.Shift.OrganizationID // Access OrganizationID from Shift
        //        });

        //    var attendanceData = await query.ToListAsync();

        //    // Organize the result in a way that matches the format needed by the front-end
        //    var formattedData = attendanceData.Select(item => new
        //    {
        //        Date = item.AttendanceDate.ToString("dd-MM-yyyy"), // Include the full date with day, month, and year
        //        Shift = item.ShiftName,
        //        CheckInTime = item.CheckInTime?.ToString("hh:mm tt"),
        //        CheckOutTime = item.CheckOutTime?.ToString("hh:mm tt"),
        //        RegularHour = item.RegularHour,
        //        OvertimeHour = item.OvertimeHour,
        //        LateHour = item.LateHour,
        //        EarlyHour = item.EarlyHour,
        //        WorkingHour = item.WorkingHour,
        //        Remarks = item.Remarks,
        //        Status = item.StatusID, // You can map this to a specific status name if needed
        //                                // Check if the current AttendanceDate is a holiday or weekend
        //        SpecialDay = GetSpecialDayLabel(item.AttendanceDate, weekendWeekdays, holidays)
        //    }).ToList();

        //    // Returning the formatted data as a JSON response
        //    return new Microsoft.AspNetCore.Mvc.JsonResult(formattedData);
        //}


    }
}
