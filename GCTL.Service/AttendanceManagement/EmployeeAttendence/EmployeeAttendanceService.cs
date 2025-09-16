using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGenericRepository<AttendanceLog> _genericAttendanceLog;
        private readonly ILocalizationContext _localizationContext;

        public EmployeeAttendanceService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, IGenericRepository<Shifts> genericRepositoryShift, IGenericRepository<Holidays> genericHolidays, IGenericRepository<WeekendDays> genericWeekdays, IGenericRepository<WeekendSettings> genericWeekSettings, IGenericRepository<EmployeeOfficeInfo> genericEmployeeOfficeInfo, IGenericRepository<AttendanceLog> genericAttendanceLog, ILocalizationContext localizationContext) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericRepositoryShift = genericRepositoryShift;
            _genericHolidays = genericHolidays;
            _genericWeekdays = genericWeekdays;
            _genericWeekSettings = genericWeekSettings;
            _genericEmployeeOfficeInfo = genericEmployeeOfficeInfo;
            _genericAttendanceLog = genericAttendanceLog;
            _localizationContext = localizationContext;
        }

        public async Task<PaginationService<Attendance, EmployeeAttendenceVM>.PaginationResult<EmployeeAttendenceVM>> GetAllAsync(
                               int pageNumber = 1,
                               int pageSize = 5,
                               string searchTerm = "",
                               string sortColumn = "OrganizationID",
                               string sortOrder = "desc",
                               int? organizationID = null,
                               int? employeeId =null, int? statusID=null, string? sortId = "") 
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
            if (statusID.HasValue && statusID.Value > 0)
            {
                query = query.Where(x => x.StatusID == statusID.Value);
            }
            // Handle sortId logic for time-based sorting
            if (!string.IsNullOrEmpty(sortId))
            {
                switch (sortId)
                {
                    case "Ascending":
                        // Sort by AttendanceDate in ascending order (earliest first)
                        query = query.OrderBy(x => x.AttendanceDate);
                        break;
                    case "Descending":
                        // Sort by AttendanceDate in descending order (latest first)
                        query = query.OrderByDescending(x => x.AttendanceDate);
                        break;
                    case "LastMonth":
                        // Filter records for the last month
                        var lastMonth = DateTime.Now.AddMonths(-1);
                        var dateOnlyLastMonth = DateOnly.FromDateTime(lastMonth);
                        query = query.Where(x => x.AttendanceDate >= dateOnlyLastMonth);
                        break;
                    case "Last7days":
                        // Filter records for the last 7 days
                        var last7Days = DateTime.Now.AddDays(-7);
                        var dateOnlyLast7Days = DateOnly.FromDateTime(last7Days);
                        query = query.Where(x => x.AttendanceDate >= dateOnlyLast7Days);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // If no sortId is specified, default to descending order by AttendanceDate
                query = query.OrderByDescending(x => x.AttendanceDate);
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
                    LateHour = FormatTime(x.LateTimeMinutes),
                    //EarlyHour = x.EarlyHour.HasValue ? x.EarlyHour.Value.ToString("F2") : "-",
                    EarlyHour = FormatTime(x.EarlyTimeMinutes),
                    RegularHour = FormatTime(x.OfficeTimeMinutes),
                    OvertimeHour = FormatTime(x.OvertimeMinutes),
                    WorkingHours = FormatTime(x.WorkingTimeMinutes),
                    Break = "-",

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

            // Format CheckInTime based on the localization context
            //var checkInFormatted = _localizationContext(checkInTime);
            // Calculate production time as the difference between current time and CheckInTime
            var productionTime = currentTime - checkInTime;

            // Convert 'TimeOnly?' to 'TimeSpan?' before applying the null-coalescing operator
            var mealBreakTime = shift.MealBreakTime.HasValue ? shift.MealBreakTime.Value.ToTimeSpan() : TimeSpan.Zero;
            var actualTotalWorkingTime = shiftEndTime - shiftStartTime - mealBreakTime;

            // Limit the production time to the shift working hours (excluding break time)
            if (productionTime > actualTotalWorkingTime.GetValueOrDefault())
            {
                productionTime = actualTotalWorkingTime.GetValueOrDefault();
            }

            // Handle overtime if the production time exceeds the shift's total working hours
            var overtime = productionTime > actualTotalWorkingTime ? productionTime - actualTotalWorkingTime : TimeSpan.Zero;

            // Format the production and overtime times
            var formattedProductionTime = productionTime.ToString(@"hh\.mm");

            // Fix for CS1503: The issue is that `overtime.HasValue.ToString(@"hh\:mm")` is incorrect because `HasValue` is a boolean, not a TimeSpan.  
            var formattedOvertime = overtime.HasValue ? overtime.Value.ToString(@"hh\.mm") : "00:00";

            var totalWorkingHourMnt = actualTotalWorkingTime.GetValueOrDefault() + overtime + mealBreakTime;

            //var manualOvertime = attendanceData.OvertimeHour?.ToString("F2"); ;

            // Fix for CS1503: Correctly convert manualOvertime from string to TimeSpan before formatting  
            var manualOvertime = attendanceData.OvertimeMinutes.HasValue
                                ? TimeSpan.FromMinutes(attendanceData.OvertimeMinutes.Value * 60 + attendanceData.OvertimeMinutes.Value)
                                : TimeSpan.Zero;


            return new EmployeeAttendenceVM
            {
                EmployeeID = attendanceData.EmployeeID,
                EmployeeName = $"{attendanceData.Employee?.FirstName} {attendanceData.Employee?.LastName}",
                AttendanceDate = attendanceData.AttendanceDate.ToString("yyyy-MM-dd"),
                CheckInTime = attendanceData.CheckInTime?.ToString("HH:mm"),
                CheckInShiftTime = shiftStartTime?.ToString("HH:mm"), // Assuming this is the same as CheckInTime
                CheckOutTime = attendanceData.CheckOutTime?.ToString("HH:mm"),
                CheckOutShiftTime = shiftEndTime?.ToString("HH:mm"), // Assuming this is the same as CheckOutTime
                RegularHour = attendanceData.OfficeTimeMinutes?.ToString(),
                OvertimeHour = attendanceData.OvertimeMinutes?.ToString(),
                LateHour = attendanceData.LateTimeMinutes?.ToString(),
                EarlyHour = attendanceData.EarlyTimeMinutes?.ToString(),
                TotalWorkingHours = FormatTimeSpanHHMM(totalWorkingHourMnt.GetValueOrDefault()),
                ActualWorkingHrsMnt = FormatTimeSpanHHMM(actualTotalWorkingTime.GetValueOrDefault()), // Convert to minutes
                CurrentTime = currentTimeString,
                ProductionTime = FormatTimeSpanHHMM(productionTime),
                ProductionTimeMinute = (productionTime.TotalMinutes).ToString("F2"), // Convert to minutes
                Overtime = FormatTimeSpanHHMM(manualOvertime),//formattedOvertime,
                Break = FormatTimeSpanHHMM(mealBreakTime) // Format meal break time
            };
        }

        public async Task<EmployeeAttendenceVM> GetAttendanceProgressBarAsync(int userId)
        {
            var today = DateTime.UtcNow;

            // Get today's attendance record
            var attendanceDataId = await _genericRepository.AllActive()
                .Where(a => a.EmployeeID == userId && a.AttendanceDate == DateOnly.FromDateTime(today))
                .FirstOrDefaultAsync();

            if (attendanceDataId == null)
            {
                return new EmployeeAttendenceVM
                {
                    TotalWorkingHours = "0h 0m",
                    ProductiveHours = "0h 0m",
                    BreakHours = "0h 0m",
                    Overtime = "0h 0m",
                    LateHours = "0h 0m",
                    EarlyHours = "0h 0m",
                    SessionTimeline = new List<SessionData>()
                };
            }

            var attendanceLogs = await _genericAttendanceLog.AllActive()
                .Where(log => log.AttendanceID == attendanceDataId.AttendanceID
                             && log.CHECKTIME_UTC.Value.Date == today.Date)
                .OrderBy(log => log.CHECKTIME_UTC)
                .ToListAsync();

            if (!attendanceLogs.Any())
            {
                return new EmployeeAttendenceVM
                {
                    TotalWorkingHours = "0h 0m",
                    ProductiveHours = "0h 0m",
                    BreakHours = "0h 0m",
                    Overtime = "0h 0m",
                    LateHours = "0h 0m",
                    EarlyHours = "0h 0m",
                    SessionTimeline = new List<SessionData>()
                };
            }

            var shift = await _genericRepositoryShift.All()
                .Where(s => s.ShiftID == attendanceDataId.ShiftID)
                .FirstOrDefaultAsync();

            var shiftStartTime = shift?.StartTime.HasValue == true
                ? DateTime.Today.Add(shift.StartTime.Value.ToTimeSpan())
                : (DateTime?)null;

            var shiftEndTime = shift?.EndTime.HasValue == true
                ? DateTime.Today.Add(shift.EndTime.Value.ToTimeSpan())
                : (DateTime?)null;

            int totalRegularMinutes = 0;
            int totalBreakMinutes = 0;
            int totalLateMinutes = 0;
            int totalEarlyMinutes = 0;
            int totalOvertimeMinutes = 0;

            var sessionTimeline = new List<SessionData>();
            var firstPunch = attendanceLogs.First().CHECKTIME_UTC.Value;

            int shiftDurationMinutes = 0;
            if (shiftStartTime.HasValue && shiftEndTime.HasValue)
                shiftDurationMinutes = (int)(shiftEndTime.Value - shiftStartTime.Value).TotalMinutes;


            // ---------------- STEP 1: Late / Early ----------------
            if (shiftStartTime.HasValue && shift.IsLateCount==true)
            {
                if (firstPunch > shiftStartTime.Value)
                {
                    var lateMinutes = (int)(firstPunch - shiftStartTime.Value).TotalMinutes;
                    if (shift.GraceTime.HasValue && lateMinutes > shift.GraceTime.Value.ToTimeSpan().TotalMinutes)
                    {
                        totalLateMinutes = lateMinutes;
                        sessionTimeline.Add(new SessionData
                        {
                            Type = "Late",
                            Duration = $"{lateMinutes / 60}h {lateMinutes % 60}m",
                            Percentage = shiftDurationMinutes > 0
                                ? $"{(int)((float)lateMinutes / shiftDurationMinutes * 100)}%"
                                : "0%"
                        });
                    }
                }
                else if (firstPunch < shiftStartTime.Value)
                {
                    var earlyMinutes = (int)(shiftStartTime.Value - firstPunch).TotalMinutes;
                    if (earlyMinutes > 5)
                    {
                        totalEarlyMinutes = earlyMinutes;
                        sessionTimeline.Add(new SessionData
                        {
                            Type = "Early",
                            Duration = $"{earlyMinutes / 60}h {earlyMinutes % 60}m",
                            Percentage = shiftDurationMinutes > 0
                                ? $"+{(int)((float)earlyMinutes / shiftDurationMinutes * 100)}%"
                                : "0%"
                        });
                    }
                }
            }

            // ---------------- STEP 2: Loop through punches ----------------
            for (int i = 0; i < attendanceLogs.Count; i++)
            {
                var log = attendanceLogs[i];

                if (i % 2 == 0) // Worked
                {
                    var endTime = (i + 1 < attendanceLogs.Count)
                        ? attendanceLogs[i + 1].CHECKTIME_UTC
                        : DateTime.UtcNow;

                    var workedMinutes = endTime.HasValue && log.CHECKTIME_UTC.HasValue
                       ? (int)(endTime.Value - log.CHECKTIME_UTC.Value).TotalMinutes
                       : 0;

                    totalRegularMinutes += workedMinutes;

                    sessionTimeline.Add(new SessionData
                    {
                        Type = "Worked",
                        Duration = $"{workedMinutes / 60}h {workedMinutes % 60}m",
                        Percentage = shiftDurationMinutes > 0
                            ? $"{(int)((float)workedMinutes / shiftDurationMinutes * 100)}%"
                            : "0%"
                    });
                }
                else // Break
                {
                    if (i + 1 < attendanceLogs.Count)
                    {
                        var breakDuration = attendanceLogs[i + 1].CHECKTIME_UTC.Value - log.CHECKTIME_UTC.Value;
                        var breakMinutes = (int)breakDuration.TotalMinutes; // Convert TimeSpan to minutes
                        totalBreakMinutes += breakMinutes;
                       
                        sessionTimeline.Add(new SessionData
                        {
                            Type = "Break",
                            Duration = $"{breakMinutes / 60}h {breakMinutes % 60}m",
                            Percentage = shiftDurationMinutes > 0
                                ? $"{(int)((float)breakMinutes / shiftDurationMinutes * 100)}%"
                                : "0%"
                        });
                    }
                }
            }

            // ---------------- STEP 3: Overtime ----------------
            var lastPunch = attendanceLogs.Last().CHECKTIME_UTC.Value;
            if (shiftEndTime.HasValue && lastPunch > shiftEndTime.Value)
            {
                totalOvertimeMinutes = (int)(lastPunch - shiftEndTime.Value).TotalMinutes;

                var lastWorked = sessionTimeline.LastOrDefault(s => s.Type == "Worked");
                if (lastWorked != null)
                {
                    var parts = lastWorked.Duration.Split(' ');
                    int workedMinutes = int.Parse(parts[0].Replace("h", "")) * 60
                                       + int.Parse(parts[1].Replace("m", ""));
                    workedMinutes += totalOvertimeMinutes;
                    lastWorked.Duration = $"{workedMinutes / 60}h {workedMinutes % 60}m";
                }

                sessionTimeline.Add(new SessionData
                {
                    Type = "Overtime",
                    Duration = $"{totalOvertimeMinutes / 60}h {totalOvertimeMinutes % 60}m",
                    Percentage = shiftDurationMinutes > 0
                        ? $"+{(int)((float)totalOvertimeMinutes / shiftDurationMinutes * 100)}%"
                        : "0%"
                });
            }

            string FormatTime(int minutes) => $"{minutes / 60}h {minutes % 60}m";

            return new EmployeeAttendenceVM
            {
                TotalWorkingHours = FormatTime(totalRegularMinutes + totalOvertimeMinutes + totalLateMinutes + totalEarlyMinutes),
                ProductiveHours = FormatTime(totalRegularMinutes),
                BreakHours = FormatTime(totalBreakMinutes),
                Overtime = FormatTime(totalOvertimeMinutes),
                LateHours = FormatTime(totalLateMinutes),
                EarlyHours = FormatTime(totalEarlyMinutes),
                SessionTimeline = sessionTimeline,

                // Fix to pass only the hour part as a string.  
                ShiftStartTime = shiftStartTime.HasValue ? shiftStartTime.Value.ToString("HH") : "-",

                

            };
        }


        //public async Task<EmployeeAttendenceVM> GetAttendanceProgressBarAsync(int userId)
        //{
        //    var today = DateTime.UtcNow;

        //    // Get today's attendance record
        //    var attendanceDataId = await _genericRepository.AllActive()
        //        .Where(a => a.EmployeeID == userId && a.AttendanceDate == DateOnly.FromDateTime(today))
        //        .FirstOrDefaultAsync();

        //    if (attendanceDataId == null)
        //    {
        //        return new EmployeeAttendenceVM
        //        {
        //            TotalWorkingHours = "0h 0m",
        //            ProductiveHours = "0h 0m",
        //            BreakHours = "0h 0m",
        //            Overtime = "0h 0m",
        //            LateHours = "0h 0m",
        //            EarlyHours = "0h 0m",
        //            SessionTimeline = new List<SessionData>()
        //        };
        //    }

        //    var attendanceLogs = await _genericAttendanceLog.AllActive()
        //        .Where(log => log.AttendanceID == attendanceDataId.AttendanceID
        //                     && log.CHECKTIME_UTC.Value.Date == today.Date)
        //        .OrderBy(log => log.CHECKTIME_UTC)
        //        .ToListAsync();

        //    if (!attendanceLogs.Any())
        //    {
        //        return new EmployeeAttendenceVM
        //        {
        //            TotalWorkingHours = "0h 0m",
        //            ProductiveHours = "0h 0m",
        //            BreakHours = "0h 0m",
        //            Overtime = "0h 0m",
        //            LateHours = "0h 0m",
        //            EarlyHours = "0h 0m",
        //            SessionTimeline = new List<SessionData>()
        //        };
        //    }

        //    var shift = await _genericRepositoryShift.All()
        //        .Where(s => s.ShiftID == attendanceDataId.ShiftID)
        //        .FirstOrDefaultAsync();

        //    var shiftStartTime = shift?.StartTime.HasValue == true
        //        ? DateTime.Today.Add(shift.StartTime.Value.ToTimeSpan())
        //        : (DateTime?)null;

        //    var shiftEndTime = shift?.EndTime.HasValue == true
        //        ? DateTime.Today.Add(shift.EndTime.Value.ToTimeSpan())
        //        : (DateTime?)null;

        //    // ---------------- STEP 1: Totals ----------------
        //    int totalRegularMinutes = 0;
        //    int totalBreakMinutes = 0;
        //    int totalLateMinutes = 0;
        //    int totalEarlyMinutes = 0;
        //    int totalOvertimeMinutes = 0;

        //    var sessionTimeline = new List<SessionData>();

        //    var firstPunch = attendanceLogs.First().CHECKTIME_UTC.Value;

        //    if (shiftStartTime.HasValue)
        //    {
        //        if (firstPunch > shiftStartTime.Value)
        //        {
        //            // Employee is late
        //            var lateMinutes = (int)(firstPunch - shiftStartTime.Value).TotalMinutes;
        //            if (lateMinutes > 5)
        //            {
        //                totalLateMinutes = lateMinutes;
        //                sessionTimeline.Add(new SessionData
        //                {
        //                    Type = "Late",
        //                    Duration = $"{lateMinutes / 60}h {lateMinutes % 60}m",
        //                    Percentage = "0%"
        //                });
        //            }
        //        }
        //        else if (firstPunch < shiftStartTime.Value)
        //        {
        //            // Employee is early
        //            var earlyMinutes = (int)(shiftStartTime.Value - firstPunch).TotalMinutes;
        //            if (earlyMinutes > 5)
        //            {
        //                totalEarlyMinutes = earlyMinutes;
        //                sessionTimeline.Add(new SessionData
        //                {
        //                    Type = "Early",
        //                    Duration = $"{earlyMinutes / 60}h {earlyMinutes % 60}m",
        //                    Percentage = "0%"
        //                });
        //            }
        //        }
        //    }

        //    // ---------------- STEP 2: Loop through punches ----------------
        //    for (int i = 0; i < attendanceLogs.Count; i++)
        //    {
        //        var log = attendanceLogs[i];

        //        if (i % 2 == 0) // Odd punch → Worked
        //        {
        //            var endTime = (i + 1 < attendanceLogs.Count)
        //                ? attendanceLogs[i + 1].CHECKTIME_UTC // Next punch (checkout)
        //                : DateTime.UtcNow; // still working

        //            var workedMinutes = endTime.HasValue && log.CHECKTIME_UTC.HasValue
        //               ? (int)(endTime.Value - log.CHECKTIME_UTC.Value).TotalMinutes
        //               : 0; // Default to 0 if either value is null.  
        //            totalRegularMinutes += workedMinutes;

        //            sessionTimeline.Add(new SessionData
        //            {
        //                Type = "Worked",
        //                Duration = $"{workedMinutes / 60}h {workedMinutes % 60}m",
        //                Percentage = "0%" // later updated
        //            });
        //        }
        //        else // Even punch → Break
        //        {
        //            if (i + 1 < attendanceLogs.Count)
        //            {
        //                var breakMinutes = (int)(attendanceLogs[i + 1].CHECKTIME_UTC.Value - log.CHECKTIME_UTC.Value).TotalMinutes;
        //                totalBreakMinutes += breakMinutes;

        //                sessionTimeline.Add(new SessionData
        //                {
        //                    Type = "Break",
        //                    Duration = $"{breakMinutes / 60}h {breakMinutes % 60}m",
        //                    Percentage = "0%"
        //                });
        //            }
        //        }
        //    }

        //    // ---------------- STEP 3: Late / Early ----------------


        //    // ---------------- STEP 4: Overtime ----------------
        //    var lastPunch = attendanceLogs.Last().CHECKTIME_UTC.Value;
        //    if (shiftEndTime.HasValue && lastPunch > shiftEndTime.Value)
        //    {
        //        totalOvertimeMinutes = (int)(lastPunch - shiftEndTime.Value).TotalMinutes;

        //        // Add overtime into last Worked session
        //        var lastWorked = sessionTimeline.LastOrDefault(s => s.Type == "Worked");
        //        if (lastWorked != null)
        //        {
        //            // Update duration
        //            var parts = lastWorked.Duration.Split(' ');
        //            int workedMinutes = int.Parse(parts[0].Replace("h", "")) * 60
        //                               + int.Parse(parts[1].Replace("m", ""));
        //            workedMinutes += totalOvertimeMinutes;

        //            lastWorked.Duration = $"{workedMinutes / 60}h {workedMinutes % 60}m";
        //        }

        //        sessionTimeline.Add(new SessionData
        //        {
        //            Type = "Overtime",
        //            Duration = $"{totalOvertimeMinutes / 60}h {totalOvertimeMinutes % 60}m",
        //            Percentage = "0%"
        //        });
        //    }

        //    // ---------------- STEP 5: Percentages ----------------
        //    int totalAll = totalRegularMinutes + totalBreakMinutes + totalLateMinutes + totalEarlyMinutes + totalOvertimeMinutes;

        //    foreach (var s in sessionTimeline)
        //    {
        //        var parts = s.Duration.Split(' ');
        //        int minutes = int.Parse(parts[0].Replace("h", "")) * 60
        //                     + int.Parse(parts[1].Replace("m", ""));

        //        if (totalAll > 0)
        //            s.Percentage = $"{(int)((float)minutes / totalAll * 100)}%";
        //    }

        //    // ---------------- STEP 6: Return ----------------
        //    string FormatTime(int minutes) => $"{minutes / 60}h {minutes % 60}m";

        //    return new EmployeeAttendenceVM
        //    {
        //        TotalWorkingHours = FormatTime(totalRegularMinutes + totalOvertimeMinutes + totalLateMinutes + totalEarlyMinutes),
        //        ProductiveHours = FormatTime(totalRegularMinutes),
        //        BreakHours = FormatTime(totalBreakMinutes),
        //        Overtime = FormatTime(totalOvertimeMinutes),
        //        LateHours = FormatTime(totalLateMinutes),
        //        EarlyHours = FormatTime(totalEarlyMinutes),
        //        SessionTimeline = sessionTimeline
        //    };
        //}








        private string FormatTimeSpanHHMM(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalHours:D2}h {timeSpan.Minutes:D2}m";
        }

        public async Task<IActionResult> GetEmployeePunchTimeline(int userId)
        {

            var punches = await _genericAttendanceLog.All()
                .Where(x => x.DeletedAt == null &&
                            x.Attendance.EmployeeID == userId &&
                            x.PunchTime.Date == DateTime.Today)
                .OrderBy(x => x.PunchTime)
                .Select(x => x.PunchTime.ToString("HH:mm"))
                .ToListAsync();

            return new JsonResult(new { punches }); // Replace 'Json' with 'JsonResult'
        }

        public async Task<List<PunchActivityDto>> GetEmployeePunchActivityAsync(int userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Pull only PunchTime from DB
            var punchTimes = await _genericAttendanceLog.All()
                .Where(x => x.DeletedAt == null
                            && x.Attendance.EmployeeID == userId
                            && x.CHECKTIME_UTC >= today
                            && x.CHECKTIME_UTC < tomorrow)
                .OrderByDescending(x => x.CHECKTIME_UTC)
                .Select(x => x.CHECKTIME_UTC)
                
                .ToListAsync();

            var result = punchTimes
               // consistent with your ViewBag logic
                .Select((x, index) =>
                {

                    var localTime = x.HasValue ? x.Value.ToOrgTime(_localizationContext) : DateTime.MinValue.ToOrgTime(_localizationContext);
                

                    var type = index % 2 == 0 ? "Punch In" : "Punch Out";

                    return new PunchActivityDto
                    {
                        Type = type,
                        Time = localTime.ToString(),
                        Description = type + " "
                    };
                }).ToList();

            

            return result;
        }

        

        public async Task<TimeOnly?> GetEmployeeFirstPunchInTimeAsync(int userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Pull the first PunchTime from DB  
            var firstPunchIn = await _genericAttendanceLog.All()
                .Where(x => x.DeletedAt == null
                            && x.Attendance.EmployeeID == userId
                            && x.CHECKTIME_UTC >= today
                            && x.CHECKTIME_UTC < tomorrow)
                .OrderBy(x => x.CHECKTIME_UTC)
                .Select(x => x.CHECKTIME_UTC)
                .FirstOrDefaultAsync();

            // If a punch-in time is found, convert it to local time  
            if (firstPunchIn.HasValue)
            {
                // Convert the UTC time to the localized time based on the _localizationContext
                var localTimeString = firstPunchIn.Value.ToOrgTime(_localizationContext); // Get local time as string

                // Parse the string into DateTime and return
                var localTime = TimeOnly.Parse(localTimeString);

                return localTime; // Return the localized DateTime
            }

            return null; // Return null if no punch-in is found  
        }


        public async Task<IActionResult> CalculateWorkingHours(int attendanceId)
        {
            var currentDate = DateTime.Today.AddDays(-1); // Yesterday’s date

            var attendanceData = await _genericRepository.All()  //attendence table
                .Where(a => a.AttendanceID==5 && a.DeletedAt == null)
                .OrderByDescending(a => a.AttendanceDate)
                .FirstOrDefaultAsync();

            // Fetch shift data for the given attendance entry
            var shift = await _genericRepositoryShift.All()
                .Where(s => s.ShiftID == attendanceData.ShiftID)
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return new JsonResult(new { message = "Shift data not found." });
            }

            var shiftStartTime = shift.StartTime; // Shift start time
            var shiftStartDateTime = currentDate.Add(shiftStartTime.HasValue ? shiftStartTime.Value.ToTimeSpan() : TimeSpan.Zero); // Shift start time  
            var shiftEndTime = shift.EndTime; // Shift end time
            var shiftEndDateTime = currentDate.Add(shiftEndTime.HasValue ? shiftEndTime.Value.ToTimeSpan() : TimeSpan.Zero); // Shift end time

            // Fetch all punches for the current employee on the given day
            var punches = await _genericAttendanceLog.All()
                .Where(x => x.AttendanceID == 5 && x.DeletedAt == null && x.PunchTime.Date == currentDate)
                .OrderBy(x => x.PunchTime)
                .ToListAsync();

            if (punches.Count < 2)
                return new JsonResult(new { message = "Not enough punches for calculation." });

            var sessions = new List<object>();
            double totalWorkingMinutes = 0;
            double totalBreakMinutes = 0;

            // Loop through the punches to calculate sessions
            for (int i = 0; i < punches.Count - 1; i += 2)
            {
                var start = punches[i].PunchTime;
                var end = punches[i + 1].PunchTime;
                double minutes = (end - start).TotalMinutes;

                // If it's a work session
                if ((i / 2) % 2 == 0)
                {
                    // Early Entry (if punch-in time is before shift start time)
                    if (start < shiftStartDateTime)
                    {
                        double earlyEntryMinutes = (shiftStartDateTime - start).TotalMinutes;
                        sessions.Add(new { type = "Early Entry", duration = FormatTimeFromMinutes(earlyEntryMinutes) });
                        totalWorkingMinutes += earlyEntryMinutes; // Add early entry time to working minutes
                    }

                    // Late Entry (if punch-out time is after shift end time)
                    if (end > shiftEndDateTime)
                    {
                        double lateEntryMinutes = (end - shiftEndDateTime).TotalMinutes;
                        sessions.Add(new { type = "Late Entry", duration = FormatTimeFromMinutes(lateEntryMinutes) });
                        totalWorkingMinutes += lateEntryMinutes; // Add late entry time to working minutes
                    }

                    // Add regular work session
                    sessions.Add(new { type = "Worked", duration = FormatTimeFromMinutes(minutes) });
                    totalWorkingMinutes += minutes;
                }
                else // Break session
                {
                    sessions.Add(new { type = "Break", duration = FormatTimeFromMinutes(minutes) });
                    totalBreakMinutes += minutes;
                }
            }

            // Handle odd count (e.g., last punch without pair)
            if (punches.Count % 2 != 0)
            {
                var lastWorkStart = punches[^1].PunchTime;
                var now = DateTime.Now;
                double minutes = (now - lastWorkStart).TotalMinutes;
                sessions.Add(new { type = "Worked", duration = $"{FormatTimeFromMinutes(minutes)} (till now)" });
                totalWorkingMinutes += minutes;

                // Check if the last punch is after the shift end time (late entry)
                if (now > shiftEndDateTime)
                {
                    double lateEntryMinutes = (now - shiftEndDateTime).TotalMinutes;
                    sessions.Add(new { type = "Late Entry", duration = FormatTimeFromMinutes(lateEntryMinutes) });
                    totalWorkingMinutes += lateEntryMinutes; // Add late entry time to working minutes
                }
            }

            double productiveMinutes = totalWorkingMinutes;
            double overtimeMinutes = productiveMinutes > 480 ? productiveMinutes - 480 : 0;

            // Prepare the result
            var result = new
            {
                sessionTimeline = sessions,
                totalWorkingHours = FormatTimeFromMinutes(totalWorkingMinutes),
                breakHours = FormatTimeFromMinutes(totalBreakMinutes),
                productiveHours = FormatTimeFromMinutes(productiveMinutes),
                overtime = FormatTimeFromMinutes(overtimeMinutes)
            };

            return new JsonResult(result);
        }

        // Helper function to format time from minutes to "Xh Ym"
        private string FormatTimeFromMinutes(double minutes)
        {
            int hours = (int)(minutes / 60);
            int mins = (int)(minutes % 60);
            return $"{hours}h {mins}m";
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
            DateTime currentDate = DateTime.UtcNow;
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

        public async Task<double> GetTotalHoursForMonth(int employeeId, int organizationId, int? organizationBranchId)
        {
            DateTime now = DateTime.UtcNow;
            DateOnly today = DateOnly.FromDateTime(now);
            DateOnly startOfMonth = new DateOnly(now.Year, now.Month, 1);

            // Attendance for this month (inclusive)
            var attendanceData = await _genericRepository.AllActive()
                .Where(a => a.EmployeeID == employeeId
                    && a.AttendanceDate >= startOfMonth
                    && a.AttendanceDate <= today
                    )
                .ToListAsync();
            double totalRegularHours = attendanceData.Sum(a => a.OfficeTimeMinutes ?? 0);
            int hours = (int)totalRegularHours;
            int minutes = (int)((totalRegularHours - hours) * 60);

            // Weekend days for this org/branch (0=Sunday ... 6=Saturday)
            var weekendDays = await _genericWeekdays.All()
                .Where(w => w.DeletedAt == null
                    && w.WeekendSetting.OrganizationID == organizationId
                    && (organizationBranchId == null || w.WeekendSetting.OrganizationBranchID == organizationBranchId))
                .Select(w => w.WeekdayNumber)
                .ToListAsync();

            // Shifts (by org)
            var shifts = await _genericRepositoryShift.All()
                .Where(s => s.OrganizationID == organizationId)
                .ToListAsync();

            // Holidays overlapping this month window
            var holidays = await _genericHolidays.All()
                .Where(h => h.OrganizationID == organizationId
                    && (organizationBranchId == null || h.OrganizationBranchID == organizationBranchId)
                    && h.StartDate.HasValue && h.EndDate.HasValue
                    && DateOnly.FromDateTime(h.EndDate.Value) >= startOfMonth
                    && DateOnly.FromDateTime(h.StartDate.Value) <= today)
                .ToListAsync();

            double totalWorkingHours = 0;

            foreach (var attendance in attendanceData)
            {
                DateOnly d = attendance.AttendanceDate;

                // Skip weekends
                if (weekendDays.Contains((int)d.DayOfWeek))
                    continue;

                // Skip holidays
                bool isHoliday = holidays.Any(h =>
                    d >= DateOnly.FromDateTime(h.StartDate!.Value) &&
                    d <= DateOnly.FromDateTime(h.EndDate!.Value));
                if (isHoliday)
                    continue;

                // Find the shift for this attendance
                var shift = shifts.FirstOrDefault(s => s.ShiftID == attendance.ShiftID);
                if (shift == null)
                    continue;

                var startTs = shift.StartTime.HasValue ? shift.StartTime.Value.ToTimeSpan() : TimeSpan.Zero;
                var endTs = shift.EndTime.HasValue ? shift.EndTime.Value.ToTimeSpan() : TimeSpan.Zero;
                var meal = shift.MealBreakTime.HasValue ? shift.MealBreakTime.Value.ToTimeSpan() : TimeSpan.Zero;

                // Raw duration (if negative, it wrapped past midnight)
                var duration = endTs - startTs;
                if (duration < TimeSpan.Zero)
                    duration += TimeSpan.FromDays(1);

                duration -= meal;
                if (duration > TimeSpan.Zero)
                    totalWorkingHours += duration.TotalHours; // keep fractional hours
            }

            return totalWorkingHours;
        }







    }
}
