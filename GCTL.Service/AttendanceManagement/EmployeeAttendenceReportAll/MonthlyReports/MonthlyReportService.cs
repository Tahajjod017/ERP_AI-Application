using Dapper;
using GCTL.Core.Enums;
using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;

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

        public MonthlyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, HolidayHelper holidayHelper, WeekendHelper weekendHelper, LeaveHelper leaveHelper) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _leaveHelper = leaveHelper;
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

        public async Task<IActionResult> GetMonthlyAttendanceReport(int? departmentId, int? organizationId, int? employeeId, string monthyear)
         {
            // Parse the month and year to filter attendance for that period
            var startDate = DateOnly.ParseExact(monthyear + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = startDate.AddMonths(1).AddDays(-1);  // To get the last day of the month

            // Get the weekend days for the organization and branch (Assuming the weekend days are defined as Sunday = 0 and Saturday = 6)
            var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId.Value, null); // Pass the branch ID if needed

            // Get active holidays within the given date range
            var holidays = _holidayHelper.GetActiveHolidays(organizationId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

            // Get leave information for the employee within the given date range
            var leaveTypes = _leaveHelper.GetLeaveDatesAndTypes(employeeId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

            // Query the Attendance and Shifts tables with the filters
            var query = _genericRepository.All()
                .AsNoTracking()  // Disable change tracking for better performance since we are only reading data
                .Include(x => x.Employee)  // Include the related Employee data
                .Include(x => x.Status)    // Include the related Status data
                .Include(x => x.Shift)     // Include the related Shift data
                .Where(x => x.DeletedAt == null &&  // Filter for deleted records
                           (employeeId == null || x.EmployeeID == employeeId) &&
                           (organizationId == null || x.Shift.OrganizationID == organizationId) &&
                           x.AttendanceDate >= startDate && x.AttendanceDate <= endDate)
                .Select(x => new
                {
                    x.AttendanceID,
                    x.EmployeeID,
                    x.AttendanceDate,
                    x.StatusID,
                    x.ShiftID,
                    x.CheckInTime,
                    x.CheckOutTime,
                    x.Remarks,
                    x.LIP,
                    x.LMAC,
                    x.OfficeTimeMinutes,
                    x.OvertimeMinutes,
                    x.LateTimeMinutes,
                    x.EarlyTimeMinutes,
                    x.WorkingTimeMinutes,
                    ShiftName = x.Shift.ShiftName,  // Access ShiftName from the Shift navigation property
                    OrganizationID = x.Shift.OrganizationID // Access OrganizationID from Shift
                });

            var attendanceData = await query.ToListAsync();

            // Total days in the month
            var totalDaysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

            // Organize the result in a way that matches the format needed by the front-end
            var formattedData = new List<AttendanceMonthlyReport>();

            for (int day = 0; day <= totalDaysInMonth; day++)
            {
                var attendanceItem = attendanceData.FirstOrDefault(item => item.AttendanceDate.Day == day);

                bool isLeaveDay = leaveTypes.Any(leave => leave.leaveDate.Day == day);

                // Here we check if the current day is a weekend (Sunday = 0, Saturday = 6)
                bool isWeekend = (startDate.AddDays(day - 1).DayOfWeek == DayOfWeek.Sunday || startDate.AddDays(day - 1).DayOfWeek == DayOfWeek.Saturday);

                bool isHoliday = holidays.Any(h =>
                    new DateTime(startDate.Year, startDate.Month, day) >= h.StartDate &&
                    new DateTime(startDate.Year, startDate.Month, day) <= h.EndDate);

                if (attendanceItem != null)
                {
                    // If attendance data is available for the day, populate it
                    formattedData.Add(new AttendanceMonthlyReport
                    {
                        Date = day.ToString("00"),
                        Shift = attendanceItem.ShiftName,
                        CheckInTime = attendanceItem.CheckInTime?.ToString("hh:mm tt"),
                        CheckOutTime = attendanceItem.CheckOutTime?.ToString("hh:mm tt"),
                        RegularHour = attendanceItem.OfficeTimeMinutes,
                        OvertimeHour = attendanceItem.OvertimeMinutes,
                        LateHour = attendanceItem.LateTimeMinutes,
                        EarlyHour = attendanceItem.EarlyTimeMinutes,
                        WorkingHour = attendanceItem.WorkingTimeMinutes,
                        Remarks = attendanceItem.Remarks,
                        Status = attendanceItem.StatusID ?? 0,  // Default status if null
                        SpecialDay = "Workday"
                    });
                }
                else if (isLeaveDay || isWeekend || isHoliday)
                {
                    // If the day is a leave day, weekend, or holiday
                    formattedData.Add(new AttendanceMonthlyReport
                    {
                        Date = day.ToString("00"),
                        Shift = "-",
                        CheckInTime = null,
                        CheckOutTime = null,
                        RegularHour = null,
                        OvertimeHour = null,
                        LateHour = null,
                        EarlyHour = null,
                        WorkingHour = null,
                        Remarks = "-",
                        Status = 0, // Status as 0 for leave, weekend, or holiday
                        SpecialDay = GetSpecialDayLabel(day, isLeaveDay, isWeekend, isHoliday)
                    });
                }
                else
                {
                    // If no data exists for the day (absent)
                    formattedData.Add(new AttendanceMonthlyReport
                    {
                        Date = day.ToString("00"),
                        Shift = "-",
                        CheckInTime = null,
                        CheckOutTime = null,
                        RegularHour = null,
                        OvertimeHour = null,
                        LateHour = null,
                        EarlyHour = null,
                        WorkingHour = null,
                        Remarks = "Absent",
                        Status = 0, // Mark as absent
                        SpecialDay = "Absent"
                    });
                }
            }

            // Returning the formatted data as a JSON response
            return new Microsoft.AspNetCore.Mvc.JsonResult(formattedData);
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
