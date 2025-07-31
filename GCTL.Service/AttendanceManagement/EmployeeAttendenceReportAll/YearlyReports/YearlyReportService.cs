using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
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
        }
        public async Task<List<YearlySpecialDayReportVM>> GetYearlySpecialDaysReport(int? departmentId, int? organizationId, int? employeeId, int year)
        {
            var yearlySpecialDaysReport = new List<YearlySpecialDayReportVM>();

            for (int month = 1; month <= 12; month++)  // Loop through each month
            {
                var startDate = new DateOnly(year, month, 1);  // Start of the month
                var endDate = startDate.AddMonths(1).AddDays(-1);  // End of the month

                // Get the weekend days for the organization and branch (weekend days like Friday = 5, Saturday = 6)
                var weekendWeekdays = _weekendHelper.GetWeekendWeekdayNumbers(organizationId.Value, null); // Pass the branch ID if needed

                // Get active holidays within the given date range
                var holidays = _holidayHelper.GetActiveHolidays(organizationId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

                // Get leave information for the employee within the given date range
                var leaveTypes = _leaveHelper.GetLeaveDatesAndTypes(employeeId.Value, startDate.ToDateTime(new TimeOnly(0, 0)), endDate.ToDateTime(new TimeOnly(0, 0)));

                // Mapped weekdays array (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
                var weekdayMapping = new Dictionary<int, List<int>>();

                // Loop through the days of the month (1 to totalDaysInMonth)
                for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                {
                    var currentDayOfWeek = startDate.AddDays(day - 1).DayOfWeek;

                    // Map the current day to the corresponding weekday array
                    if (!weekdayMapping.ContainsKey((int)currentDayOfWeek))
                    {
                        weekdayMapping[(int)currentDayOfWeek] = new List<int>();
                    }

                    // Add the calendar day to the corresponding weekday
                    weekdayMapping[(int)currentDayOfWeek].Add(day);
                }

                var specialDaysForMonth = new List<SpecialDay>();

                // Now, let's iterate through each day of the month and generate the special days report
                for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                {
                    bool isLeaveDay = leaveTypes.Any(leave => leave.leaveDate.Day == day);
                    bool isWeekend = weekendWeekdays.Contains((int)startDate.AddDays(day - 1).DayOfWeek);
                    bool isHoliday = holidays.Any(h =>
                        new DateTime(year, month, day) >= h.StartDate &&
                        new DateTime(year, month, day) <= h.EndDate);

                    string specialDay = string.Empty;
                    if (isLeaveDay)
                    {
                        specialDay = "Leave";
                    }
                    else if (isWeekend)
                    {
                        specialDay = "Weekend";
                    }
                    else if (isHoliday)
                    {
                        specialDay = "Holiday";
                    }
                    else
                    {
                        specialDay = "Workday";
                    }

                    // Add the special day entry for the current day of the month
                    specialDaysForMonth.Add(new SpecialDay
                    {
                        Date = day.ToString("00"),  // Ensure date is formatted as '01', '02', etc.
                        SpecialDays = specialDay
                    });
                }

                // Add the month report to the yearly report
                yearlySpecialDaysReport.Add(new YearlySpecialDayReportVM
                {
                    Month = startDate.ToString("MMMM"),  // Get month name
                    SpecialDays = specialDaysForMonth
                });
            }

            return yearlySpecialDaysReport;
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
            return "Workday";
        }

       

        

    }
}
