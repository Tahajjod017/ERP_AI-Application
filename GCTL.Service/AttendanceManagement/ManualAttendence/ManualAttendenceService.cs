using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

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

        public ManualAttendenceService(IGenericRepository<EmployeeOfficeInfo> officialRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<AttendanceLog> attendenceLogRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<Shifts> shiftRepository, IGenericRepository<Attendance> attendenceRepository, IGenericRepository<LeaveApplications> leaveRepository)
        {
            _officialRepository = officialRepository;
            _employeeRepository = employeeRepository;
            _attendenceLogRepository = attendenceLogRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _shiftRepository = shiftRepository;
            _attendenceRepository = attendenceRepository;
            _leaveRepository = leaveRepository;
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

               
                if (attendances == null || !attendances.Any())
                    return new List<AttendanceRecord>();

                var attendanceList = (from att in attendances
                                      where att != null // Filter out null attendance records

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

                                      select new AttendanceRecord
                                      {
                                          EmployeeId = emp?.EmployeeID ?? 0,
                                          Id = att?.AttendanceID ?? 0,
                                          EmployeeName = emp != null ?
                                              $"{emp.FirstName ?? ""} {emp.LastName ?? ""}".Trim() : "N/A",
                                          EmployeeRole = des?.DesignationName ?? "N/A",
                                          Department = dept?.DepartmentName ?? "N/A",
                                          EmployeeImage = !string.IsNullOrEmpty(emp?.EmployeeImageFileName)
                                              ? imgTemFolder + emp.EmployeeImageFileName
                                              : "https://placehold.co/300x200?text=Photo",
                                          AttendanceDate = att?.AttendanceDate.ToString("dd MMM yyyy") ?? "N/A",
                                          ScheduleTime = shift != null ?
                                              $"{shift.StartTime:hh\\:mm tt} - {shift.EndTime:hh\\:mm tt}" : "N/A",
                                          ActualInTime = att?.CheckInTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          ActualOutTime = att?.CheckOutTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          BreakInTime = shift?.MealBreakStartTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          BreakOutTime = shift?.MealBreakEndTime?.ToString("hh:mm tt") ?? "Not Punched",
                                          Overtime = (att?.OvertimeHour ?? 0) > 0 ?
                                              $"{att.OvertimeHour} hrs" : "No Overtime",
                                          BiometricHits = att != null ?
                                              logs?.Count(x => x.AttendanceID == att.AttendanceID) ?? 0 : 0,

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




        //public async Task<List<AttendanceRecord>> GetAbnormalPunchDataAsync(string imgTemFolder)
        //{
        //    var allData = await GetAllDataAsync(imgTemFolder);
        //    var abnormalList = new List<AttendanceRecord>();

        //    foreach (var record in allData)
        //    {
        //        var reasons = new List<string>();
        //        string type = "";
        //        int punchCount = record.PunchData?.Count ?? 0;

        //        // Skip full and partial leave records unless punches exist during full leave
        //        if (record.IsOnFullLeave && punchCount > 0)
        //        {
        //            reasons.Add("Punches recorded during full leave");
        //            type = "Leave Violation";
        //        }
        //        else if (record.IsOnFullLeave || record.IsPartialLeave)
        //        {
        //            continue; // Skip valid leave records with no punches
        //        }

        //        // 1. Punch Count Issues
        //        if (punchCount == 0)
        //        {
        //            reasons.Add("No punch recorded");
        //            type = "Punch Count";
        //        }
        //        else if (punchCount == 1)
        //        {
        //            var punch = record.PunchData.First();
        //            reasons.Add($"Single punch: {punch.Label} at {punch.Time}");
        //            type = "Punch Count";
        //        }
        //        else if (punchCount % 2 != 0)
        //        {
        //            reasons.Add("Unpaired punches (odd count)");
        //            type = "Punch Sequence";
        //        }

        //    // 2. Punch Sequence Validation
        //    if (punchCount >= 2)
        //    {
        //        var punches = record.PunchData.OrderBy(x => DateTime.ParseExact(x.Time, "hh:mm tt", null)).ToList();
        //        var logs1 = await _attendenceLogRepository.GetAllAsync();
        //        var recordLogs1 = logs1.Where(x => x.AttendanceID == record.Id).OrderBy(x => x.PunchTime).ToList();

        //        for (int i = 0; i < punches.Count - 1; i++)
        //        {
        //            bool isFirstIn = i % 2 == 0; // Even index = IN, Odd index = OUT
        //            bool isSecondIn = (i + 1) % 2 == 0;
        //            if (isFirstIn == isSecondIn)
        //            {
        //                var punchType = isFirstIn ? "IN" : "OUT";
        //                reasons.Add($"Consecutive {punchType} punches at {punches[i].Time} and {punches[i + 1].Time}");
        //                type = type == "" ? "Punch Sequence" : type;
        //            }
        //        }
        //        if (punches.Count % 2 != 0)
        //        {
        //            var lastPunchType = (punches.Count - 1) % 2 == 0 ? "IN" : "OUT";
        //            reasons.Add($"Last punch is {lastPunchType} (unpaired)");
        //            type = type == "" ? "Punch Sequence" : type;
        //        }
        //    }


        //    // 3. Break Validation
        //    if (record.ScheduleTime != "N/A" && punchCount >= 2)
        //        {
        //            var breakStart = record.BreakInTime != "Not Punched" ? DateTime.ParseExact(record.BreakInTime, "hh:mm tt", null).TimeOfDay : (TimeSpan?)null;
        //            var breakEnd = record.BreakOutTime != "Not Punched" ? DateTime.ParseExact(record.BreakOutTime, "hh:mm tt", null).TimeOfDay : (TimeSpan?)null;
        //            if (breakStart.HasValue && breakEnd.HasValue)
        //            {
        //                var breakDuration = breakEnd.Value - breakStart.Value;
        //                if (breakDuration > TimeSpan.FromHours(1)) // Assume 1-hour max break
        //                {
        //                    reasons.Add($"Extended break: {breakDuration.TotalMinutes:F0} minutes");
        //                    type = type == "" ? "Break Violation" : type;
        //                }
        //            }
        //            else if (record.MinimumWorkHour.HasValue && record.MinimumWorkHour.Value.ToTimeSpan() > TimeSpan.FromHours(6))
        //            {
        //                reasons.Add("No break punches recorded for long shift");
        //                type = type == "" ? "Break Violation" : type;
        //            }
        //        }

        //        // 4. Timing Issues
        //        if (punchCount >= 2 && record.ScheduleTime != "N/A")
        //        {
        //            var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
        //            var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
        //            //var shiftStart = TimeSpan.Parse(record.ScheduleTime.Split('-')[0].Trim());
        //            //var shiftEnd = TimeSpan.Parse(record.ScheduleTime.Split('-')[1].Trim());

        //            var shiftStart = SafeParseTime(record.ScheduleTime.Split('-')[0].Trim());
        //            var shiftEnd = SafeParseTime(record.ScheduleTime.Split('-')[1].Trim());

        //            if (record.GraceTime.HasValue)
        //            {
        //                var graceSpan = record.GraceTime.Value.ToTimeSpan();
        //                if (firstPunchTime > shiftStart + graceSpan)
        //                {
        //                    var lateBy = (firstPunchTime - shiftStart).TotalMinutes;
        //                    reasons.Add($"Late check-in by {lateBy:F0} minutes");
        //                    type = type == "" ? "Timing" : type;
        //                }
        //                if (lastPunchTime < shiftEnd - graceSpan)
        //                {
        //                    var earlyBy = (shiftEnd - lastPunchTime).TotalMinutes;
        //                    reasons.Add($"Early departure by {earlyBy:F0} minutes");
        //                    type = type == "" ? "Timing" : type;
        //                }
        //            }
        //        }

        //        // 5. Work Duration
        //        if (punchCount >= 2)
        //        {
        //            var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
        //            var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;
        //            var workDuration = lastPunchTime - firstPunchTime;

        //            if (record.MinimumWorkHour.HasValue)
        //            {
        //                var minWorkSpan = record.MinimumWorkHour.Value.ToTimeSpan();
        //                if (workDuration < minWorkSpan)
        //                {
        //                    reasons.Add($"Work duration too short: {workDuration.TotalHours:F1} hrs vs {minWorkSpan.TotalHours:F1} hrs required");
        //                    type = type == "" ? "Duration" : type;
        //                }
        //            }
        //        }

        //        // 6. Overtime Validation
        //        if (record.Overtime != "No Overtime")
        //        {
        //            var overtimeHours = double.Parse(record.Overtime.Split(' ')[0]);
        //            if (!record.isOvertimeEligible)
        //            {
        //                reasons.Add("Unauthorized overtime recorded");
        //                type = type == "" ? "Overtime" : type;
        //            }
        //            else if (record.MaximumOverTime.HasValue && overtimeHours > record.MaximumOverTime.Value.ToTimeSpan().TotalHours)
        //            {
        //                reasons.Add($"Overtime exceeds limit: {overtimeHours:F1} hrs vs {record.MaximumOverTime.Value.ToTimeSpan().TotalHours:F1} hrs allowed");
        //                type = type == "" ? "Overtime" : type;
        //            }
        //            else if (record.MinimumOverTime.HasValue && overtimeHours < record.MinimumOverTime.Value.ToTimeSpan().TotalHours)
        //            {
        //                reasons.Add($"Overtime below minimum: {overtimeHours:F1} hrs vs {record.MinimumOverTime.Value.ToTimeSpan().TotalHours:F1} hrs required");
        //                type = type == "" ? "Overtime" : type;
        //            }
        //        }

        //        // 7. Rapid Punches
        //        if (punchCount >= 2)
        //        {
        //            var punches = record.PunchData.OrderBy(x => DateTime.ParseExact(x.Time, "hh:mm tt", null)).ToList();
        //            for (int i = 0; i < punches.Count - 1; i++)
        //            {
        //                var timeDiff = DateTime.ParseExact(punches[i + 1].Time, "hh:mm tt", null) - DateTime.ParseExact(punches[i].Time, "hh:mm tt", null);
        //                if (timeDiff.TotalSeconds < 30)
        //                {
        //                    reasons.Add($"Rapid punches detected: {punches[i].Time} and {punches[i + 1].Time}");
        //                    type = type == "" ? "Biometric Anomaly" : type;
        //                }
        //            }
        //        }

        //        // 8. Retroactive Punches
        //        var logs = await _attendenceLogRepository.GetAllAsync();
        //        var recordLogs = logs.Where(x => x.AttendanceID == record.Id).ToList();
        //        foreach (var log in recordLogs)
        //        {
        //            if (log.CreatedAt > log.PunchTime.AddDays(1))
        //            {
        //                reasons.Add($"Retroactive punch added on {log.CreatedAt:dd MMM yyyy}");
        //                type = type == "" ? "Data Integrity" : type;
        //            }
        //        }

        //        // 9. Unscheduled Work or No Shift
        //        if (record.ScheduleTime == "N/A")
        //        {
        //            reasons.Add("Punch recorded without assigned shift");
        //            type = type == "" ? "Schedule Violation" : type;
        //        }

        //        // 10. Cross-Day Shift Handling
        //        if (punchCount == 1 && record.ScheduleTime != "N/A")
        //        {
        //            // var shiftEnd = TimeSpan.Parse(record.ScheduleTime.Split('-')[1].Trim());
        //            var shiftEnd = SafeParseTime(record.ScheduleTime.Split('-')[1].Trim());
        //            if (shiftEnd < TimeSpan.FromHours(12)) // Likely a night shift
        //            {
        //                reasons.Add("Possible incomplete cross-day shift");
        //                type = type == "" ? "Cross-Day Shift" : type;
        //            }
        //        }

        //        // 11. Group Punch Check
        //        var groupedPunches = allData
        //            .Where(r => r.PunchData != null && r.PunchData.Any())
        //            .SelectMany(r => r.PunchData.Select(p => new { EmployeeId = r.EmployeeId, Time = p.Time }))
        //            .GroupBy(p => p.Time)
        //            .Where(g => g.Count() > 3) // Flag if >3 employees punch at same time
        //            .ToList();

        //        foreach (var group in groupedPunches)
        //        {
        //            foreach (var punch in group)
        //            {
        //                var record1 = allData.FirstOrDefault(r => r.EmployeeId == punch.EmployeeId);
        //                if (record1 != null && !abnormalList.Contains(record1))
        //                {
        //                    record.PossibleReason = $"Suspicious group punch at {punch.Time} with {group.Count()} employees";
        //                    record.AbnormalType = "Group Punch";
        //                    abnormalList.Add(record1);
        //                }
        //            }
        //        }

        //        // Add to abnormal list if issues found
        //        if (reasons.Any())
        //        {
        //            record.PossibleReason = string.Join(", ", reasons);
        //            record.AbnormalType = type;
        //            abnormalList.Add(record);
        //        }
        //    }

        //    return abnormalList;
        //}


    


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
                        var graceSpan = record.GraceTime.Value.ToTimeSpan();
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
                    var minWorkSpan = record.MinimumWorkHour.Value.ToTimeSpan();

                    if (workDuration < minWorkSpan)
                    {
                        reasons.Add($"Work duration too short: {workDuration.TotalHours:F1} hrs vs {minWorkSpan.TotalHours:F1} hrs required");
                        //type = type == "" ? "Duration" : type;
                        type = type == "" ? ViolationType.Duration.ToString() : type;
                    }
                }

                // 6. Unauthorized Overtime
                if (record.Overtime != "No Overtime" && !record.isOvertimeEligible)
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

        //private TimeSpan SafeParseTime(string timeString)
        //{
        //    if (string.IsNullOrWhiteSpace(timeString))
        //        return TimeSpan.Zero;

        //    try
        //    {
        //        // First try parsing as DateTime (handles AM/PM format)
        //        if (DateTime.TryParse(timeString, out DateTime dateTime))
        //        {
        //            return dateTime.TimeOfDay;
        //        }

        //        // Fallback: try direct TimeSpan parsing for 24-hour format
        //        if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
        //        {
        //            return timeSpan;
        //        }

        //        return TimeSpan.Zero;
        //    }
        //    catch (Exception)
        //    {
        //        return TimeSpan.Zero;
        //    }
        //}









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



    }
}
