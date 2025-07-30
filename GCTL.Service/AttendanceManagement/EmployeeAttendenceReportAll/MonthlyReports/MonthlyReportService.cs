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
    public class MonthlyReportService:AppService<Attendance>
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;

        public MonthlyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, HolidayHelper holidayHelper, WeekendHelper weekendHelper) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
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


            // Get the weekend days for the organization and branch
            var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId.Value, null); // Pass the branch ID if needed

            // Get active holidays within the given date range
            var holidays = _holidayHelper.GetActiveHolidays(organizationId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

            // Query the Attendance and Shifts tables with the filters
            var query = _genericRepository.All()
                .AsNoTracking()  // Disable change tracking for better performance since we are only reading data
                .Include(x => x.Employee)  // Include the related Employee data
                .Include(x => x.Status)    // Include the related Status data
                .Include(x => x.Shift)     // Include the related Shift data
                .Where(x => x.DeletedAt == null &&  // Filter for deleted records
                           (employeeId == null || x.EmployeeID == employeeId) &&
                           (organizationId == null || x.Shift.OrganizationID == organizationId) &&
                           //(departmentId == null || x.Employee.DepartmentID == departmentId) &&
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
                    x.RegularHour,
                    x.OvertimeHour,
                    x.LateHour,
                    x.EarlyHour,
                    x.WorkingHour,
                    ShiftName = x.Shift.ShiftName,  // Access ShiftName from the Shift navigation property
                    OrganizationID = x.Shift.OrganizationID // Access OrganizationID from Shift
                });

            var attendanceData = await query.ToListAsync();

            // Organize the result in a way that matches the format needed by the front-end
            var formattedData = attendanceData.Select(item => new
            {
                Date = item.AttendanceDate.ToString("dd"),
                Shift = item.ShiftName,
                CheckInTime = item.CheckInTime?.ToString("hh:mm tt"),
                CheckOutTime = item.CheckOutTime?.ToString("hh:mm tt"),
                RegularHour = item.RegularHour,
                OvertimeHour = item.OvertimeHour,
                LateHour = item.LateHour,
                EarlyHour = item.EarlyHour,
                WorkingHour = item.WorkingHour,
                Remarks = item.Remarks,
                Status = item.StatusID, // You can map this to a specific status name if needed
                                        // Check if the current AttendanceDate is a holiday or weekend
                SpecialDay = GetSpecialDayLabel(item.AttendanceDate, weekendWeekdays, holidays)
            }).ToList();

            // Returning the formatted data as a JSON response
            return new Microsoft.AspNetCore.Mvc.JsonResult(formattedData);
        }

        // Helper method to get the label for weekends and holidays
        private string GetSpecialDayLabel(DateOnly attendanceDate, List<int> weekendWeekdays, List<Holidays> holidays)
        {
            // Check if the date is a weekend day
            int weekdayNumber = (int)attendanceDate.DayOfWeek; // 0=Sunday, 1=Monday, ..., 6=Saturday

            // Check if it's a weekend
            if (weekendWeekdays.Contains(weekdayNumber))
            {
                return "W"; // Weekend takes priority
            }

            // Check if it's a holiday (only if it's not a weekend)
            var holiday = holidays.FirstOrDefault(h => h.StartDate.Value.Date <= attendanceDate.ToDateTime(new TimeOnly(0, 0)).Date && h.EndDate.Value.Date >= attendanceDate.ToDateTime(new TimeOnly(0, 0)).Date);
            if (holiday != null)
            {
                return "H"; // Holiday
            }

            return ""; // No special day
        }


    }
}
