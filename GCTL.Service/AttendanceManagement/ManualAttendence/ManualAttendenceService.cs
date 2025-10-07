using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using GCTL.Data.Models;
using GCTL.Service.AdminSettings.GeneralSettings;
using Microsoft.EntityFrameworkCore;
using static GCTL.Service.AdminSettings.GeneralSettings.UtcTimeHelper;

namespace GCTL.Service.AttendanceManagement.ManualAttendence
{
    public class ManualAttendenceService : IManualAttendenceService
    {

        private readonly IGenericRepository<EmployeeOfficeInfo> _officialRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<AttendanceLog> _attendenceLogRepository;
        private readonly IGenericRepository<Attendance> _attendenceRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<Shifts> _shiftRepository;
        private readonly IGenericRepository<LeaveApplications> _leaveRepository;
        private readonly ILocalizationContext _localizationContext;

        public ManualAttendenceService(IGenericRepository<EmployeeOfficeInfo> officialRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<AttendanceLog> attendenceLogRepository, IGenericRepository<Attendance> attendenceRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<Shifts> shiftRepository, IGenericRepository<LeaveApplications> leaveRepository, ILocalizationContext localizationContext)
        {
            _officialRepository = officialRepository;
            _employeeRepository = employeeRepository;
            _attendenceLogRepository = attendenceLogRepository;
            _attendenceRepository = attendenceRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _shiftRepository = shiftRepository;
            _leaveRepository = leaveRepository;
            _localizationContext = localizationContext;
        }

        public async Task<List<AttendanceRecord>> GetAllDataAsync(string imgTemFolder)
        {
            try
            {
               
                var employees = await _employeeRepository.GetAllAsync();
                var official = await _officialRepository.GetAllAsync(); 
                var attendances = await _attendenceRepository.GetAllAsync(); 
                var logs = await _attendenceLogRepository.GetAllAsync();   
                var departments = await _departmentRepository.GetAllAsync();
                var designations = await _designationRepository.GetAllAsync();
                var shifts = await _shiftRepository.GetAllAsync();
                var leaves = await _leaveRepository.GetAllAsync();

                var today = DateOnly.FromDateTime(DateTime.Today);

                

                if (attendances == null || !attendances.Any())
                    return new List<AttendanceRecord>();

                var attendanceList = (from att in attendances
                                      where att != null && att.IsChecked != true && att.AttendanceDate < today  // Filter out unchecked attendance records
                                                                                                                // where att != null // Filter out null attendance records

                                      join emp in employees on att.EmployeeID equals emp.EmployeeID into empGroup
                                      from emp in empGroup.DefaultIfEmpty()

                                      join shift in shifts on att.ShiftID equals shift.ShiftID into shiftGroup
                                      from shift in shiftGroup.DefaultIfEmpty()

                                      join off in official on emp?.EmployeeID equals off.EmployeeID into offGroup
                                      from off in offGroup.DefaultIfEmpty()

                                      join dept in departments on off?.DepartmentID equals dept.DepartmentID into deptGroup
                                      from dept in deptGroup.DefaultIfEmpty()

                                      join des in designations on off?.DesignationID equals des.DesignationID into desGroup
                                      from des in desGroup.DefaultIfEmpty()

                                      join lev in leaves on att.EmployeeID equals lev.EmployeeID into levGroup
                                      from lev in levGroup.DefaultIfEmpty()

                    // StartTime = x.StartTime.HasValue ? TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(x.StartTime.Value, _localizationContext) : null,
                    //EndTime = x.EndTime.HasValue ? TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(x.EndTime.Value, _localizationContext) : null,
                    

                                      select new AttendanceRecord
                                      {
                                          EmployeeId = emp?.EmployeeID ?? 0,
                                          Id = att?.AttendanceID ?? 0,
                                          EmployeeName = emp != null ? $"{emp.FirstName ?? ""} {emp.LastName ?? ""}".Trim() : "N/A",
                                          EmployeeRole = des?.DesignationName ?? "N/A",
                                          Department = dept?.DepartmentName ?? "N/A",
                                          EmployeeImage = !string.IsNullOrEmpty(emp?.EmployeeImageFileName) ? imgTemFolder + emp.EmployeeImageFileName : "https://placehold.co/300x200?text=Photo",
                                          AttendanceDate = att?.AttendanceDate.ToString("dd MMM yyyy") ?? "N/A",
                                          //ScheduleTime = shift != null ? $"{shift.StartTime:hh\\:mm tt} - {shift.EndTime:hh\\:mm tt}" : "N/A",
                                          //ActualInTime = att?.CheckInTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          //ActualOutTime = att?.CheckOutTime?.ToString("hh:mm tt") ?? "Not Punched",

                                          ScheduleTime = shift.StartTime.HasValue && shift.EndTime.HasValue ? $"{TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(shift.StartTime.Value, _localizationContext)} - {TimeConversionHelper.ConvertUtcTimeOnlyToLocalFormatted(shift.EndTime.Value, _localizationContext)}" : "N/A",

                                          ActualInTime = att?.CheckInTime.HasValue == true    ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(att.CheckInTime.Value, DateTimeKind.Utc), _localizationContext)    : "Not Punched",

                                          ActualOutTime = att?.CheckOutTime.HasValue == true    ? TimeConversionHelper.ConvertUtcDateTimeToLocalHHmm(DateTime.SpecifyKind(att.CheckOutTime.Value, DateTimeKind.Utc), _localizationContext)    : "Not Punched",



                                          BreakInTime = shift?.MealBreakStartTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          BreakOutTime = shift?.MealBreakEndTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          //Overtime = (att?.OvertimeHour ?? 0) > 0 ?
                                          //$"{att.OvertimeHour} hrs" : "No Overtime",
                                          Overtime = att?.OvertimeMinutes,
                                          BiometricHits = att != null ? logs?.Count(x => x.AttendanceID == att.AttendanceID) ?? 0 : 0,

                                          PossibleReason = "",
                                          GraceTime = shift?.GraceTime,
                                          MinimumWorkHour = shift?.MinimumWorkingTime,
                                          isOvertimeEligible = shift?.IsAllowOvertime ?? false,
                                          MinimumOverTime = shift?.MinimumRequiredOvertime,
                                          MaximumOverTime = shift?.MaximumAllowedOvertime,

                                          IsOnFullLeave = lev != null &&
                                                        att != null &&
                                                        lev.StatusID == 2 &&
                                                        lev.IsFullDay &&
                                                        lev.FromDate != null &&
                                                        lev.ToDate != null &&
                                                        att.AttendanceDate >= lev.FromDate &&
                                                        att.AttendanceDate <= lev.ToDate,

                                          IsPartialLeave = lev != null &&
                                                         att != null &&
                                                         lev.StatusID == 2 &&
                                                         !lev.IsFullDay &&
                                                         lev.FromDate != null &&
                                                         att.AttendanceDate == lev.FromDate,

                                          PartialLeaveTimeRange = lev != null &&
                                                                lev.StatusID == 2 &&
                                                                !lev.IsFullDay &&
                                                                lev.PartialFromTime != null &&
                                                                lev.PartialToTime != null
                                              ? $"{lev.PartialFromTime:hh\\:mm tt} - {lev.PartialToTime:hh\\:mm tt}"
                                              : null,

                                          PunchData = att != null && logs != null ? logs
                                              .Where(x => x.AttendanceID == att.AttendanceID && x != null)
                                              .OrderBy(x => x.PunchTime)
                                              .Select(x => new PunchData
                                              {
                                                  Time = x.PunchTime.ToString("hh:mm tt"),
                                                  Label = "punch",
                                                  Icon = "fas fa-fingerprint",
                                                  Deletable = false
                                              }).ToList() : new List<PunchData>()
                                      }).ToList();

                return attendanceList;
            }
            catch (NullReferenceException ex)
            {
                // Log the specific error for debugging
                Console.WriteLine($"Null reference exception in GetAllDataAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                Console.WriteLine($"General exception in GetAllDataAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AttendanceRecord>> GetAbnormalPunchDataAsync(string imgTemFolder)
        {
            var allData = await GetAllDataAsync(imgTemFolder);
            var abnormalList = new HashSet<AttendanceRecord>(); // Use HashSet to avoid duplicates

            foreach (var record in allData)
            {
                var reasons = new List<string>();
                string type = "";

                // 1. Punches on Leave Days (Full or Partial)
                if (record.IsOnFullLeave || record.IsPartialLeave)
                {
                    int punchCount1 = record.PunchData?.Count ?? 0;
                    if (punchCount1 > 0)
                    {
                        reasons.Add($"Punches recorded during {(record.IsOnFullLeave ? "full" : "partial")} leave");
                      //  type = "Leave Violation";
                        type = ViolationType.LeaveViolation.ToString();
                    }
                }

                // 2. Punches without Assigned Shift
                if (record.ScheduleTime == "N/A" && (record.PunchData?.Count ?? 0) > 0)
                {
                    reasons.Add("Punch recorded without assigned shift");
                    //type = type == "" ? "Schedule Violation" : type;
                    type = type == "" ? ViolationType.ScheduleViolation.ToString() : type;
                }

                int punchCount = record.PunchData?.Count ?? 0;

                // 3. Odd Punch Count (Detect IN or OUT Missing)
                if (punchCount % 2 != 0)
                {
                    var lastPunch = record.PunchData.Last();
                    bool isLastPunchIn = punchCount % 2 == 1; // Odd count means last punch is IN (expecting OUT next)
                    reasons.Add($"Unpaired punch: Last punch is {(isLastPunchIn ? "IN" : "OUT")}");
                    //type = isLastPunchIn ? "OUT Missing" : "IN Missing";
                    type = isLastPunchIn ? ViolationType.OutMissing.ToString() : ViolationType.InMissing.ToString();
                }

                // 4. Late Punch or Early Out
                if (punchCount >= 2 && record.ScheduleTime != "N/A")
                {
                    var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                    var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
                    var shiftStart = SafeParseTime(record.ScheduleTime.Split('-')[0].Trim());
                    var shiftEnd = SafeParseTime(record.ScheduleTime.Split('-')[1].Trim());

                    if (record.GraceTime.HasValue)
                    {
                        TimeSpan graceSpan = TimeSpan.FromMinutes((double)record.GraceTime);
                        //var graceSpan = record.GraceTime.Value.ToTimeSpan();
                        if (firstPunchTime > shiftStart + graceSpan)
                        {
                            var lateBy = (firstPunchTime - shiftStart).TotalMinutes;
                            reasons.Add($"Late check-in by {lateBy:F0} minutes");
                            //type = type == "" ? "Timing" : type;
                            type = type == "" ? ViolationType.Timing.ToString() : type;
                        }
                        if (lastPunchTime < shiftEnd - graceSpan)
                        {
                            var earlyBy = (shiftEnd - lastPunchTime).TotalMinutes;
                            reasons.Add($"Early departure by {earlyBy:F0} minutes");
                            //type = type == "" ? "Timing" : type;
                            type = type == "" ? ViolationType.Timing.ToString() : type;
                        }
                    }
                }

                // 5. Incomplete Work Hours (even punches but less than minimum)
                if (punchCount >= 2 && punchCount % 2 == 0 && record.MinimumWorkHour.HasValue)
                {
                    var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                    var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
                    var workDuration = lastPunchTime - firstPunchTime;

                    TimeSpan minWorkSpan = TimeSpan.FromMinutes((double)record.MinimumWorkHour);
                    //var minWorkSpan = record.MinimumWorkHour.Value.ToTimeSpan();

                    if (workDuration < minWorkSpan)
                    {
                        reasons.Add($"Work duration too short: {workDuration.TotalHours:F1} hrs vs {minWorkSpan.TotalHours:F1} hrs required");
                        //type = type == "" ? "Duration" : type;
                        type = type == "" ? ViolationType.Duration.ToString() : type;
                    }
                }

                // 6. Unauthorized Overtime
                if (record.Overtime.HasValue && !record.isOvertimeEligible)
                {
                    reasons.Add("Unauthorized overtime recorded");
                    //type = type == "" ? "Overtime" : type;
                    type = type == "" ? ViolationType.Overtime.ToString() : type;
                }

                // Add to abnormal list if issues found
                if (reasons.Any())
                {
                    record.PossibleReason = string.Join(", ", reasons);
                    record.AbnormalType = type;
                    abnormalList.Add(record);
                }
            }

            return abnormalList.ToList();
        }

        private TimeSpan SafeParseTime(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return TimeSpan.Zero;

            try
            {
                if (DateTime.TryParse(timeString, out DateTime dateTime))
                {
                    return dateTime.TimeOfDay;
                }
                if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
                {
                    return timeSpan;
                }
                return TimeSpan.Zero;
            }
            catch (Exception)
            {
                return TimeSpan.Zero;
            }
        }




        public async Task<Dictionary<string, int>> GetAbnormalTypeCountsAsync(string imgTemFolder)
        {
            var allData = await GetAllDataAsync(imgTemFolder);
            var abnormalTypeCounts = new Dictionary<string, int>();

            foreach (var record in allData)
            {
                var (type, reasons) = GetAbnormalTypeAndReasons(record);

                if (!string.IsNullOrEmpty(type))
                {
                    if (abnormalTypeCounts.ContainsKey(type))
                        abnormalTypeCounts[type]++;
                    else
                        abnormalTypeCounts[type] = 1;
                }
            }

            return abnormalTypeCounts;
        }



        private (string Type, List<string> Reasons) GetAbnormalTypeAndReasons(AttendanceRecord record)
        {
            var reasons = new List<string>();
            string type = "";
            int punchCount = record.PunchData?.Count ?? 0;

            if (record.IsOnFullLeave || record.IsPartialLeave)
            {
                if (punchCount > 0)
                {
                    reasons.Add($"Punches recorded during {(record.IsOnFullLeave ? "full" : "partial")} leave");
                    type = ViolationType.LeaveViolation.ToString();
                }
            }

            if (record.ScheduleTime == "N/A" && punchCount > 0)
            {
                reasons.Add("Punch recorded without assigned shift");
                type = type == "" ? ViolationType.ScheduleViolation.ToString() : type;
            }

            if (punchCount % 2 != 0)
            {
                bool isLastPunchIn = punchCount % 2 == 1;
                reasons.Add($"Unpaired punch: Last punch is {(isLastPunchIn ? "IN" : "OUT")}");
                type = isLastPunchIn ? ViolationType.OutMissing.ToString() : ViolationType.InMissing.ToString();
            }

            if (punchCount >= 2 && record.ScheduleTime != "N/A")
            {
                var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
                var shiftStart = SafeParseTime(record.ScheduleTime.Split('-')[0].Trim());
                var shiftEnd = SafeParseTime(record.ScheduleTime.Split('-')[1].Trim());

                if (record.GraceTime.HasValue)
                {
                    TimeSpan graceSpan = TimeSpan.FromMinutes((double)record.GraceTime);
                    //var graceSpan = record.GraceTime.Value.ToTimeSpan();

                    if (firstPunchTime > shiftStart + graceSpan)
                    {
                        var lateBy = (firstPunchTime - shiftStart).TotalMinutes;
                        reasons.Add($"Late check-in by {lateBy:F0} minutes");
                        type = type == "" ? ViolationType.Timing.ToString() : type;
                    }

                    if (lastPunchTime < shiftEnd - graceSpan)
                    {
                        var earlyBy = (shiftEnd - lastPunchTime).TotalMinutes;
                        reasons.Add($"Early departure by {earlyBy:F0} minutes");
                        type = type == "" ? ViolationType.Timing.ToString() : type;
                    }
                }
            }

            if (punchCount >= 2 && punchCount % 2 == 0 && record.MinimumWorkHour.HasValue)
            {
                var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
                var workDuration = lastPunchTime - firstPunchTime;

                TimeSpan minWorkSpan = TimeSpan.FromMinutes((double)record.MinimumWorkHour);
                //var minWorkSpan = record.MinimumWorkHour.Value.ToTimeSpan();

                if (workDuration < minWorkSpan)
                {
                    reasons.Add($"Work duration too short: {workDuration.TotalHours:F1} hrs vs {minWorkSpan.TotalHours:F1} hrs required");
                    type = type == "" ? ViolationType.Duration.ToString() : type;
                }
            }

            if (record.Overtime.HasValue && !record.isOvertimeEligible)
            {
                reasons.Add("Unauthorized overtime recorded");
                type = type == "" ? ViolationType.Overtime.ToString() : type;
            }

            return (type, reasons);
        }




        private string GetPossibleReason(Attendance att, List<AttendanceLog> logs)
        {
            var punchCount = logs.Count(x => x.AttendanceID == att.AttendanceID);

            if (!att.CheckInTime.HasValue && !att.CheckOutTime.HasValue)
                return "No IN/OUT";

            if (punchCount < 2)
                return "Insufficient Punches";

            return "OK";
        }


        private string GetBreakIn(List<AttendanceLog> logs, int attendanceId)
        {
            var breakIn = logs.Where(x => x.AttendanceID == attendanceId)
                              .OrderBy(x => x.PunchTime)
                              .Skip(1)
                              .FirstOrDefault(); // Example logic

            return breakIn?.PunchTime.ToString("hh:mm tt") ?? "Not Punched";
        }

        private string GetBreakOut(List<AttendanceLog> logs, int attendanceId)
        {
            var breakOut = logs.Where(x => x.AttendanceID == attendanceId)
                               .OrderByDescending(x => x.PunchTime)
                               .Skip(1)
                               .FirstOrDefault(); // Example logic

            return breakOut?.PunchTime.ToString("hh:mm tt") ?? "Not Punched";
        }

        public async Task< CommonReturnViewModel> SaveManualAttendance(ManualAttendanceViewModel model)
        {


            var returnModel = new CommonReturnViewModel();

            if (!int.TryParse(model.EmployeeName, out var empId) || !DateOnly.TryParse(model.AttendanceDate, out var attDate))
            {
               
                returnModel.Success = false;
                returnModel.Message = "Invalid employeeId or attendanceDate";
                return returnModel;
            }

            try
            {
                if (model == null)
                {
                    returnModel.Success = false;
                    returnModel.Message = "Invalid model data.";
                    return returnModel;
                }
                // Validate required fields
                if (string.IsNullOrEmpty(model.EmployeeName) || string.IsNullOrEmpty(model.AttendanceDate))
                {
                    returnModel.Success = false;
                    returnModel.Message = "Employee Name and Attendance Date are required.";
                    return returnModel;
                }

                var attendence = _attendenceRepository.AllActive().FirstOrDefault(x => x.EmployeeID == empId &&
                                         x.AttendanceDate == attDate);


                List<AttendanceLog> attendanceLogList = new List<AttendanceLog>();




                DateTime formattedInTime = (DateTime)FormatTimeAuto(model.ActualInTime, attDate);
                DateTime formattedOutTime = (DateTime)FormatTimeAuto(model.ActualOutTime , attDate);

                if (attendence == null)
                {
                    attendence = new Attendance
                    {
                        EmployeeID = empId,
                        AttendanceDate = attDate,
                        CreatedAt = DateTime.Now,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                        IsChecked = true // Mark as checked
                    };
                    await _attendenceRepository.AddAsync(attendence);
                }


                if (model.BreakInTime != null)
                {
                    // Parse BreakInTime as duration (assuming BreakInTime string like "00:30" meaning 30 minutes)
                    TimeOnly parsedBreakTime = TimeOnly.Parse(model.BreakInTime);
                    TimeSpan breakDuration = TimeSpan.FromHours(parsedBreakTime.Hour)
                                            + TimeSpan.FromMinutes(parsedBreakTime.Minute)
                                            + TimeSpan.FromSeconds(parsedBreakTime.Second);


                    // Get shift info
                    var shift = _attendenceRepository.AllActive()
                        .Include(o => o.Shift)
                        .Include(i => i.Employee)
                        .FirstOrDefault(x => x.EmployeeID == empId);

                    var mealBreakStartTime = shift?.Shift?.MealBreakStartTime;

                    // Calculate break window
                    DateOnly referenceDate = attDate; // or use InTime.Date, depending on your context
                    DateTime finalBreakStart;
                    DateTime finalBreakEnd;




                    if (mealBreakStartTime.HasValue)
                    {
                        // Build datetime for break start from attendance date and meal break time
                        finalBreakStart = new DateTime(referenceDate.Year, referenceDate.Month, referenceDate.Day,
                                                       mealBreakStartTime.Value.Hour,
                                                       mealBreakStartTime.Value.Minute,
                                                       mealBreakStartTime.Value.Second);

                        finalBreakEnd = finalBreakStart + breakDuration;

                        // Check if break falls within in-out window
                        if (finalBreakStart < formattedInTime || finalBreakEnd > formattedOutTime)
                        {
                            // Fall back to centered break if shift break doesn't fit
                            TimeSpan totalSpan = formattedOutTime - formattedInTime;
                            finalBreakStart = formattedInTime + TimeSpan.FromTicks(totalSpan.Ticks / 2 - breakDuration.Ticks / 2);
                            finalBreakEnd = finalBreakStart + breakDuration;
                        }
                    }
                    else
                    {
                        // No predefined shift break, calculate centered break
                        TimeSpan totalSpan = formattedOutTime - formattedInTime;
                        finalBreakStart = formattedInTime + TimeSpan.FromTicks(totalSpan.Ticks / 2 - breakDuration.Ticks / 2);
                        finalBreakEnd = finalBreakStart + breakDuration;
                    }



                    finalBreakEnd = finalBreakStart + breakDuration;


                    TimeSpan totalWorkSpan = formattedOutTime - formattedInTime;


                    if (breakDuration > totalWorkSpan)
                    {
                        returnModel.Success = false;
                        returnModel.Message = "Break duration cannot exceed total work duration.";
                        return returnModel;
                    }


                    // Add break logs if break duration is valid
                    if (breakDuration > TimeSpan.Zero)
                    {
                        var breakLogIn = new AttendanceLog
                        {
                            AttendanceID = attendence.AttendanceID,
                            PunchTime = finalBreakStart, // Break start time
                            SourceType = "ManualFormBIN",
                            CreatedAt = DateTime.Now,
                            LIP = model.LIP,
                            LMAC = model.LMAC,
                            CreatedBy = model.CreatedBy
                        };
                        var breakLogOut = new AttendanceLog
                        {
                            AttendanceID = attendence.AttendanceID,
                            PunchTime = finalBreakEnd, // Break end time
                            SourceType = "ManualFormBOUT",
                            CreatedAt = DateTime.Now,
                            LIP = model.LIP,
                            LMAC = model.LMAC,
                            CreatedBy = model.CreatedBy
                        };
                        attendanceLogList.Add(breakLogIn);
                        attendanceLogList.Add(breakLogOut);
                    }

                }

                

                



               

              

                var attdLog = new AttendanceLog
                {
                    AttendanceID = attendence.AttendanceID,
                    PunchTime = formattedInTime, // Use current time for manual attendance
                    SourceType = "ManualFormIN",
                    CreatedAt = DateTime.Now,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedBy = model.CreatedBy
                };

                attendanceLogList.Add(attdLog);


                var atdLogOut = new AttendanceLog
                {
                    AttendanceID = attendence.AttendanceID,
                    PunchTime = formattedOutTime, // Use current time for manual attendance
                    SourceType = "ManualFormOut",
                    CreatedAt = DateTime.Now,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                     CreatedBy = model.CreatedBy
                };

                attendanceLogList.Add(atdLogOut);


                

                await _attendenceLogRepository.AddRangeAsync(attendanceLogList);



                returnModel.Success = true;
                returnModel.Message = "Manual attendance saved successfully.";
                returnModel.Data = attendence.AttendanceID;

                return returnModel;
            }
            catch (Exception ex)
            {
                returnModel.Success = false;
                returnModel.Message = $"Error saving manual attendance: {ex.Message}";
                return returnModel;
            }
        }


        private DateTime? FormatTimeAuto(string time, DateOnly attendanceDate)
        {
            if (string.IsNullOrWhiteSpace(time)) return null;

            if (DateTime.TryParse(time, out DateTime parsedTime))
            {
                // Combine parsed time with attendance date
                var finalDateTime = new DateTime(
                    attendanceDate.Year,
                    attendanceDate.Month,
                    attendanceDate.Day,
                    parsedTime.Hour,
                    parsedTime.Minute,
                    parsedTime.Second
                );

                return finalDateTime;
            }

            return null;
        }


        


    }
}
