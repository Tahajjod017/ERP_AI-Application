using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MovementReports
{
    public class MovementReportService : AppService<Attendance>
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        private readonly LeaveHelper _leaveHelper;
        public MovementReportService(IGenericRepository<Attendance> genericRepository, IUserInfoService userInfoService, IGenericRepository<Shifts> genericRepositoryShift, HolidayHelper holidayHelper, WeekendHelper weekendHelper, LeaveHelper leaveHelper) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepositoryShift = genericRepositoryShift;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _leaveHelper = leaveHelper;
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
                    x.RegularHour,
                    x.OvertimeHour,
                    x.LateHour,
                    x.EarlyHour,
                    x.WorkingHour,
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
                        RegularHour = attendanceItem.RegularHour,
                        OvertimeHour = attendanceItem.OvertimeHour,
                        LateHour = attendanceItem.LateHour,
                        EarlyHour = attendanceItem.EarlyHour,
                        WorkingHour = attendanceItem.WorkingHour,
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
       
    }
}
