using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.YearlyReports
{
    public class YearlyReportService : AppService<Attendance>, IYearlyReportService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly HolidayHelper _holidayHelper;
        private readonly WeekendHelper _weekendHelper;
        private readonly LeaveHelper _leaveHelper;

        public YearlyReportService(IGenericRepository<Attendance> genericRepository, IUserInfoService userInfoService, IGenericRepository<Shifts> genericRepositoryShift, HolidayHelper holidayHelper, WeekendHelper weekendHelper, LeaveHelper leaveHelper) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepositoryShift = genericRepositoryShift;
            _holidayHelper = holidayHelper;
            _weekendHelper = weekendHelper;
            _leaveHelper = leaveHelper;
            _genericRepository = genericRepository;
        }

        public async Task<List<YearlySpecialDayReportVM>> GetYearlySpecialDaysReport(int? departmentId, int? organizationId, int? employeeId, int year)
        {
            var yearlySpecialDaysReport = new List<YearlySpecialDayReportVM>();  // A list to store monthly reports

            // Loop through each month (1 to 12)
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateOnly(year, month, 1);  // Start of the month
                var endDate = startDate.AddMonths(1).AddDays(-1);  // End of the month

                // Get the weekend days for the organization and branch
                var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId ?? 0, null);  // Safely handle null organizationId

                // Get active holidays within the given date range
                var holidays = _holidayHelper.GetActiveHolidays(organizationId ?? 0, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

                // Get leave information for the employee within the given date range
                var leaveTypes = _leaveHelper.GetLeaveDatesAndTypes(employeeId ?? 0, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

                // Query the Attendance and Shifts tables with the filters
                var query = _genericRepository.All()
                    .AsNoTracking()
                    .Include(x => x.Employee)
                    .Include(x => x.Status)
                    .Include(x => x.Shift)
                    .Where(x => x.DeletedAt == null &&
                               (employeeId == null || x.EmployeeID == employeeId) &&
                               x.AttendanceDate >= startDate && x.AttendanceDate <= endDate)
                    .Select(x => new
                    {
                        x.AttendanceID,
                        x.EmployeeID,
                        x.AttendanceDate,
                        x.CheckInTime,
                        x.CheckOutTime,
                    });

                var attendanceData = await query.ToListAsync();

                // Total days in the month
                var totalDaysInMonth = DateTime.DaysInMonth(year, month);

                // Initialize counters for total present, absent, leave, weekend, and holiday
                int totalPresent = 0;
                int totalAbsent = 0;
                int totalLeave = 0;
                int totalWeekend = 0;
                int totalHoliday = 0;

                // Use a dictionary for faster lookup by AttendanceDate.Day
                var attendanceLookup = attendanceData.ToDictionary(x => x.AttendanceDate.Day);

                // List to store the special days for the current month
                var specialDaysForMonth = new List<SpecialDay>();

                // Now, let's iterate through each day of the month and generate the special days report
                for (int day = 1; day <= totalDaysInMonth; day++)
                {
                    // Find the attendance item for the specific day
                    var attendanceItem = attendanceLookup.ContainsKey(day) ? attendanceLookup[day] : null;

                    bool isLeaveDay = leaveTypes.Any(leave => leave.leaveDate.Day == day);
                    bool isWeekend = weekendWeekdays.Contains((int)startDate.AddDays(day - 1).DayOfWeek);
                    bool isHoliday = holidays.Any(h =>
                        new DateTime(year, month, day) >= h.StartDate &&
                        new DateTime(year, month, day) <= h.EndDate);

                    string specialDay = "Absent"; // Default to absent

                    if (attendanceItem != null && attendanceItem.CheckInTime != null && attendanceItem.CheckOutTime != null)
                    {
                        specialDay = "Present";
                        totalPresent++;  // Increment total present count
                    }
                    else if (isLeaveDay)
                    {
                        specialDay = "Leave";
                        totalLeave++;  // Increment total leave count
                    }
                    else if (isWeekend)
                    {
                        specialDay = "Weekend";
                        totalWeekend++;  // Increment total weekend count
                    }
                    else if (isHoliday)
                    {
                        specialDay = "Holiday";
                        totalHoliday++;  // Increment total holiday count
                    }
                    else
                    {
                        totalAbsent++;  // Increment total absent count
                    }

                    // Add the special day entry for the current day of the month
                    specialDaysForMonth.Add(new SpecialDay
                    {
                        Date = day.ToString("00"),  // Ensure date is formatted as '01', '02', etc.
                        SpecialDays = specialDay
                    });
                }

                // Add missing days (for months with less than 31 days) as "-"
                for (int day = totalDaysInMonth + 1; day <= 31; day++)
                {
                    specialDaysForMonth.Add(new SpecialDay
                    {
                        Date = day.ToString("00"),
                        SpecialDays = "-"
                    });
                }

                // Add the month report to the yearly report, including totals for present, absent, leave, weekend, and holiday
                yearlySpecialDaysReport.Add(new YearlySpecialDayReportVM
                {
                    Month = startDate.ToString("MMMM"),  // Get month name
                    SpecialDays = specialDaysForMonth,
                    TotalPresent = totalPresent,
                    TotalAbsent = totalAbsent,
                    TotalLeave = totalLeave,
                    TotalWeekend = totalWeekend,
                    TotalHoliday = totalHoliday
                });
            }

            return yearlySpecialDaysReport;  // Return the final report
        }






    }
}
