using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendence
{
    public class EmployeeAttendanceService:AppService<Attendance>, IEmployeeAttendanceReport
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly IGenericRepository<WeekendDays> _genericWeekdays;
        private readonly IGenericRepository<WeekendSettings> _genericWeekSettings;
        private readonly IGenericRepository<Holidays> _genericHolidays;
        private readonly IGenericRepository<EmployeeOfficeInfo> _genericEmployeeOfficeInfo;

        public EmployeeAttendanceService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, IGenericRepository<Shifts> genericRepositoryShift, IGenericRepository<Holidays> genericHolidays, IGenericRepository<WeekendDays> genericWeekdays, IGenericRepository<WeekendSettings> genericWeekSettings, IGenericRepository<EmployeeOfficeInfo> genericEmployeeOfficeInfo) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryShift = genericRepositoryShift;
            _genericHolidays = genericHolidays;
            _genericWeekdays = genericWeekdays;
            _genericWeekSettings = genericWeekSettings;
            _genericEmployeeOfficeInfo = genericEmployeeOfficeInfo;
        }

        public async Task<PaginationService<Attendance, EmployeeAttendenceVM>.PaginationResult<EmployeeAttendenceVM>> GetAllAsync(
                               int pageNumber = 1,
                               int pageSize = 5,
                               string searchTerm = "",
                               string sortColumn = "OrganizationID",
                               string sortOrder = "desc",
                               int? organizationID = null,
                               int? employeeId =null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.Status)
                .Include(x => x.Shift)
                .Where(x => x.DeletedAt == null);

            //if (organizationID.HasValue && organizationID.Value > 0)
            //{
            //    query = query.Where(x => x.WeekendSetting.Organization.OrganizationID == organizationID.Value);
            //}

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(x => x.EmployeeID == employeeId.Value);
            }

            var result = await PaginationService<Attendance, EmployeeAttendenceVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%")
                          ,
                x => new EmployeeAttendenceVM
                {
                    AttendanceID = x.AttendanceID,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? x.CheckInTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    CheckOutTime = x.CheckOutTime.HasValue ? x.CheckOutTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    //LateHour = x.LateHour.HasValue ? x.LateHour.Value.ToString("F2") : "-",
                    LateHour = x.LateHour.HasValue ? (x.LateHour.Value * 60).ToString("0") : "-",
                    //EarlyHour = x.EarlyHour.HasValue ? x.EarlyHour.Value.ToString("F2") : "-",
                    EarlyHour = x.EarlyHour.HasValue ? (x.EarlyHour.Value * 60).ToString("0") : "-",


                    OvertimeHour = x.OvertimeHour.HasValue ? x.OvertimeHour.Value.ToString("F2") : "-",
                    WorkingHours = "-",
                    Break = "-",

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

        public async Task<EmployeeAttendenceVM> GetAttendanceDetailsAsync(int userId)
        {
            var currentTime = DateTime.Now;
            var currentTimeString = currentTime.ToString("HH:mm");

            // Get the most recent attendance record for the user
            var attendanceData = await _genericRepository.All()
                .Where(a => a.EmployeeID == userId && a.DeletedAt == null)
                .OrderByDescending(a => a.AttendanceDate)
                .FirstOrDefaultAsync();

            // If no attendance data or the attendance data is not for today
            if (attendanceData == null || attendanceData.AttendanceDate != DateOnly.FromDateTime(currentTime))
            {
                return new EmployeeAttendenceVM
                {
                    CurrentTime = currentTimeString,
                    CheckInTime = null,
                    ProductionTime = "0.00", //hrs
                    Overtime = "0.00", //hrs
                    TotalWorkingHours = "0" //hrs
                };
            }

            // Get the corresponding shift details for the employee's attendance
            var shift = await _genericRepositoryShift.All()
                .Where(s => s.ShiftID == attendanceData.ShiftID)
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return new EmployeeAttendenceVM
                {
                    CurrentTime = currentTimeString,
                    ProductionTime = "0.00", //hrs
                    Overtime = "0.00",
                    TotalWorkingHours = "0"
                };
            }

            // Get the CheckInTime and calculate the total working time
            var checkInTime = attendanceData.CheckInTime ?? DateTime.Now;
            var shiftStartTime = shift.StartTime;
            var shiftEndTime = shift.EndTime;

            // Calculate production time as the difference between current time and CheckInTime
            var productionTime = currentTime - checkInTime;

            // Convert 'TimeOnly?' to 'TimeSpan?' before applying the null-coalescing operator
            var mealBreakTime = shift.MealBreakTime.HasValue ? shift.MealBreakTime.Value.ToTimeSpan() : TimeSpan.Zero;
            var totalWorkingTime = shiftEndTime - shiftStartTime - mealBreakTime;

            // Limit the production time to the shift working hours (excluding break time)
            if (productionTime > totalWorkingTime.GetValueOrDefault())
            {
                productionTime = totalWorkingTime.GetValueOrDefault();
            }

            // Handle overtime if the production time exceeds the shift's total working hours
            var overtime = productionTime > totalWorkingTime ? productionTime - totalWorkingTime : TimeSpan.Zero;

            // Format the production and overtime times
            var formattedProductionTime = productionTime.ToString(@"hh\.mm");

            // Fix for CS1503: The issue is that `overtime.HasValue.ToString(@"hh\:mm")` is incorrect because `HasValue` is a boolean, not a TimeSpan.  
            var formattedOvertime = overtime.HasValue ? overtime.Value.ToString(@"hh\.mm") : "00:00";

            return new EmployeeAttendenceVM
            {
                EmployeeID = attendanceData.EmployeeID,
                EmployeeName = $"{attendanceData.Employee?.FirstName} {attendanceData.Employee?.LastName}",
                AttendanceDate = attendanceData.AttendanceDate.ToString("yyyy-MM-dd"),
                CheckInTime = attendanceData.CheckInTime?.ToString("HH:mm"),
                CheckOutTime = attendanceData.CheckOutTime?.ToString("HH:mm"),
                RegularHour = attendanceData.RegularHour?.ToString(),
                OvertimeHour = attendanceData.OvertimeHour?.ToString(),
                LateHour = attendanceData.LateHour?.ToString(),
                EarlyHour = attendanceData.EarlyHour?.ToString(),
                TotalWorkingHours = totalWorkingTime.GetValueOrDefault().TotalHours.ToString("F2"),
                CurrentTime = currentTimeString,
                ProductionTime = formattedProductionTime,
                ProductionTimeMinute = (productionTime.TotalMinutes).ToString("F2"), // Convert to minutes
                Overtime = formattedOvertime
            };
        }

        //public async Task<EmployeeAttendenceVM> GetAttendanceDetailsAsync(int userId)
        //{
        //    var currentTime = DateTime.Now; // Get current time
        //    var currentTimeString = currentTime.ToString("HH:mm"); // Format for display

        //    // Get the most recent attendance record for the user
        //    var attendanceData = await _genericRepository.All()
        //        .Where(a => a.EmployeeID == userId && a.DeletedAt == null)
        //        .OrderByDescending(a => a.AttendanceDate)
        //        .FirstOrDefaultAsync();

        //    if (attendanceData == null || attendanceData.AttendanceDate != DateOnly.FromDateTime(currentTime))
        //    {
        //        return new EmployeeAttendenceVM
        //        {
        //            CurrentTime = currentTimeString,
        //            CheckInTime = null,
        //            ProductionTime = "0.00",//hrs
        //            Overtime = "0.00",//hrs
        //            TotalWorkingHours = "0" //hrs
        //        };
        //    }

        //    // Get the corresponding shift details for the employee's attendance
        //    var shift = await _genericRepositoryShift.All()
        //        .Where(s => s.ShiftID == attendanceData.ShiftID)
        //        .FirstOrDefaultAsync();

        //    if (shift == null)
        //    {
        //        return new EmployeeAttendenceVM
        //        {
        //            CurrentTime = currentTimeString,
        //            ProductionTime = "0.00",//hrs
        //            Overtime = "0.00",
        //            TotalWorkingHours = "0"
        //        };
        //    }

        //    // Get the CheckInTime and calculate the total working time
        //    var checkInTime = attendanceData.CheckInTime ?? DateTime.Now;
        //    var shiftStartTime = shift.StartTime;
        //    var shiftEndTime = shift.EndTime;

        //    // Calculate production time as the difference between current time and CheckInTime
        //    var productionTime = currentTime - checkInTime;

        //    // Convert 'TimeOnly?' to 'TimeSpan?' before applying the null-coalescing operator
        //    var mealBreakTime = shift.MealBreakTime.HasValue ? shift.MealBreakTime.Value.ToTimeSpan() : TimeSpan.Zero;
        //    var totalWorkingTime = shiftEndTime - shiftStartTime - mealBreakTime;

        //    // Limit the production time to the shift working hours (excluding break time)
        //    if (productionTime > totalWorkingTime.GetValueOrDefault())
        //    {
        //        productionTime = totalWorkingTime.GetValueOrDefault();
        //    }

        //    // Handle overtime if the production time exceeds the shift's total working hours
        //    var overtime = productionTime > totalWorkingTime ? productionTime - totalWorkingTime : TimeSpan.Zero;

        //    // Format the production and overtime times
        //    var formattedProductionTime = productionTime.ToString(@"hh\.mm");
        //    // Fix for CS1503: The issue is that `overtime.HasValue.ToString(@"hh\:mm")` is incorrect because `HasValue` is a boolean, not a TimeSpan.  
        //    // Correcting the code to use `overtime` directly if it has a value, and format it properly.  

        //    var formattedOvertime = overtime.HasValue ? overtime.Value.ToString(@"hh\.mm") : "00:00";

        //    return new EmployeeAttendenceVM
        //    {
        //        EmployeeID = attendanceData.EmployeeID,
        //        EmployeeName = $"{attendanceData.Employee?.FirstName} {attendanceData.Employee?.LastName}",
        //        AttendanceDate = attendanceData.AttendanceDate.ToString("yyyy-MM-dd"),
        //        CheckInTime = attendanceData.CheckInTime?.ToString("HH:mm"),
        //        CheckOutTime = attendanceData.CheckOutTime?.ToString("HH:mm"),
        //        RegularHour = attendanceData.RegularHour?.ToString(),
        //        OvertimeHour = attendanceData.OvertimeHour?.ToString(),
        //        LateHour = attendanceData.LateHour?.ToString(),
        //        EarlyHour = attendanceData.EarlyHour?.ToString(),
        //        //Break = attendanceData.Break?.ToString(),
        //        //WorkingHours = attendanceData.WorkingHours?.ToString(),
        //        TotalWorkingHours = totalWorkingTime.GetValueOrDefault().TotalHours.ToString("F2"),
        //        CurrentTime = currentTimeString,
        //        ProductionTime = formattedProductionTime,
        //        ProductionTimeMinute = (productionTime.TotalMinutes).ToString("F2"), // Convert to minutes
        //        Overtime = formattedOvertime
        //    };
        //}

        public async Task<double> GetTotalHoursForWeek(int employeeId, int organizationId, int? organizationBranchId)
        {
            DateTime currentDate = DateTime.Now;
            DateOnly currentDateOnly = DateOnly.FromDateTime(currentDate); // Use DateOnly for date without time
            DateOnly startOfWeek = currentDateOnly.AddDays(-7); // Get the start of the week (7 days ago)

            var attendanceData = await _genericRepository.All()
                                .Where(a => a.EmployeeID == employeeId
                                    && a.AttendanceDate >= startOfWeek
                                    && a.AttendanceDate <= currentDateOnly
                                    && a.DeletedAt == null
                                   )
                                .ToListAsync();

            // Get employee attendance for the past 7 days, excluding weekends and holidays, and match the organizationId and organizationBranchId
            //        var attendanceData = await _genericRepository.All()
            //.Where(a => a.EmployeeID == employeeId
            //            && a.AttendanceDate >= startOfWeek
            //            && a.AttendanceDate <= currentDateOnly
            //            && a.DeletedAt == null
            //            && a.Employee.EmployeeOfficeInfoCreatedByNavigation
            //                .Any(e => e.OrganizationID == organizationId
            //                          && (organizationBranchId == null || e.OrganizationBranchID == organizationBranchId)))
            //.ToListAsync();

            //var getEmployeeOfficeInfo = await _genericEmployeeOfficeInfo.All()
            //    .Where(e => e.EmployeeID == employeeId && e.OrganizationID == organizationId && (e.OrganizationBranchID ==null || e.OrganizationBranchID == organizationBranchId) )
            //    .FirstOrDefaultAsync();


            // Get the weekend days for the current week, filtered by organization and branch
            var weekendDays = await _genericWeekdays.All()
                .Where(w => w.DeletedAt == null
                        && w.WeekendSetting.OrganizationID == organizationId
                        && (organizationBranchId == null || w.WeekendSetting.OrganizationBranchID == organizationBranchId))
                .Select(w => w.WeekdayNumber) // Get the weekday numbers (0 for Sunday, 1 for Monday, etc.)
                .ToListAsync();

            // Get the employee's shifts, filtered by organization (no need to filter by organizationBranchId for shifts)
            var shifts = await _genericRepositoryShift.All()
                .Where(s => s.OrganizationID == organizationId)  // Only filter by OrganizationID
                .ToListAsync();

            // Get the holidays for the week, filtered by organization and branch
            var holidays = await _genericHolidays.All()
                .Where(h => h.OrganizationID == organizationId
                            && (organizationBranchId == null || h.OrganizationBranchID == organizationBranchId)
                            && DateOnly.FromDateTime(h.StartDate.Value) <= currentDateOnly
                            && DateOnly.FromDateTime(h.EndDate.Value) >= startOfWeek)
                .ToListAsync();

            double totalWorkingHours = 0;

            foreach (var attendance in attendanceData)
            {
                DateOnly attendanceDateOnly = attendance.AttendanceDate;

                // Check if the day is a weekend (using WeekendDays table)
                bool isWeekend = weekendDays.Contains((int)attendanceDateOnly.DayOfWeek); // WeekendDays are stored as numbers (0 to 6)

                // Skip weekends
                if (isWeekend)
                    continue;

                // Check if the day is a holiday
                bool isHoliday = holidays.Any(h => attendanceDateOnly >= DateOnly.FromDateTime(h.StartDate.Value) && attendanceDateOnly <= DateOnly.FromDateTime(h.EndDate.Value));

                // Skip holidays
                if (isHoliday)
                    continue;

                // Get the shift for the attendance day
                var shift = shifts.FirstOrDefault(s => s.ShiftID == attendance.ShiftID);

                if (shift != null)
                {
                    // Calculate total working hours for the shift
                    var shiftStartTime = shift.StartTime;
                    var shiftEndTime = shift.EndTime;

                    // Subtract meal break time if any
                    // Replace the following line:  
                   // var mealBreakTime = shift.MealBreakTime ?? TimeSpan.Zero;

                
                    var mealBreakTime = shift.MealBreakTime.HasValue ? shift.MealBreakTime.Value.ToTimeSpan() : TimeSpan.Zero;
                    //var mealBreakTime = shift.MealBreakTime ?? TimeSpan.Zero;

                    //  total working hours for this day (shift time minus break time)
                   // var dailyWorkingHours = shiftEndTime.Subtract(shiftStartTime).Subtract(mealBreakTime).TotalHours;
                   var dailyWorkingHours = (shiftEndTime - shiftStartTime - mealBreakTime).Value.Hours;



                    totalWorkingHours += dailyWorkingHours;
                }
            }
            
            return totalWorkingHours;
        }






    }
}
