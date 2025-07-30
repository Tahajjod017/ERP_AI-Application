using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ManualAttendence;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.BiDi.Modules.Script;

namespace GCTL_App.Controllers.AttendanceManagement.ManualAttendence
{
    public class ManualAttendenceController : BaseController
    {
        private readonly IManualAttendenceService _manualAttendenceService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Attendance> _attendanceRepository;
        private readonly IGenericRepository<AttendanceLog> _attendanceLogRepository;


        public ManualAttendenceController(ITranslateService translateService, IUserProfileService userProfileService, IManualAttendenceService manualAttendenceService, IGenericRepository<Attendance> attendanceRepository, IGenericRepository<AttendanceLog> attendanceLogRepository, IGenericRepository<GCTL.Data.Models.Employees> employeeRepository) : base(translateService, userProfileService)
        {
            _manualAttendenceService = manualAttendenceService;
            _attendanceRepository = attendanceRepository;
            _attendanceLogRepository = attendanceLogRepository;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index()
        {
            SetSmartPageCode(111400);

            var violationTypeList = Enum.GetValues(typeof(ViolationType))
                .Cast<ViolationType>()
                .Select(v => new
                {
                    Value = v.ToString(),
                    Text = v.GetType()
                             .GetMember(v.ToString())
                             .First()
                             .GetCustomAttribute<DisplayAttribute>()
                             ?.Name ?? v.ToString()
                });

            ViewBag.ViolationTypeList = new SelectList(violationTypeList, "Value", "Text");

            ViewBag.EmployeeDD = new SelectList(_employeeRepository.All().Select(e => new
            {
                id = e.EmployeeID,
                name = e.FirstName + " " + e.LastName
            }), "id", "name");

            return View();
        }



        #region Dummy Data For test

        private static List<AttendanceRecord> _attendanceData = new List<AttendanceRecord>
        {
            new AttendanceRecord
            {
                Id = 1,
                EmployeeName = "Faruk Hasan",
                EmployeeRole = "Admin",
                Department = "IT",
                EmployeeImage = "https://placehold.co/300x200?text=Placeholder",
                AttendanceDate = "20 Jul 2025",
                ScheduleTime = "09:00 AM - 06:00 PM",
                ActualInTime = "09:45 AM",
                ActualOutTime = "Not Punched",
                BreakInTime = "01:00 PM",
                BreakOutTime = "Not Punched",
                Overtime = "No Overtime",
                BiometricHits = 3,
                PossibleReason = "Break In/Out Missing",
                PunchData = new List<PunchData>
                {
                    new PunchData { Time = "09:45 AM", Label = "in time", Icon = "fas fa-fingerprint", Deletable = false },
                    new PunchData { Time = "01:00 PM", Label = "break in", Icon = "fas fa-fingerprint", Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    new PunchData { Time = "06:10 PM", Label = "out time", Icon = "fas fa-fingerprint", Deletable = false }
                }
            },
            
            new AttendanceRecord
            {
                Id = 1,
                EmployeeName = "Faruk Hasan",
                EmployeeRole = "Admin",
                Department = "IT",
                EmployeeImage = "https://placehold.co/300x200?text=Placeholder",
                AttendanceDate = "20 Jul 2025",
                ScheduleTime = "09:00 AM - 06:00 PM",
                ActualInTime = "09:45 AM",
                ActualOutTime = "Not Punched",
                BreakInTime = "01:00 PM",
                BreakOutTime = "Not Punched",
                Overtime = "No Overtime",
                BiometricHits = 3,
                PossibleReason = "Break In/Out Missing",
                PunchData = new List<PunchData>
                {
                    new PunchData { Time = "09:45 AM", Label = "in time", Icon = "fas fa-fingerprint", Deletable = false },
                    new PunchData { Time = "01:00 PM", Label = "break in", Icon = "fas fa-fingerprint", Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    //new PunchData { Time = "", Label = "break out", Icon = "fas fa-times", NotPunched = true, Deletable = false },
                    new PunchData { Time = "06:10 PM", Label = "out time", Icon = "fas fa-fingerprint", Deletable = false }
                }
            },
            


        };

        private static readonly Dictionary<string, int> _summary = new Dictionary<string, int>
        {

            { "inTimeMissing", 29 },
            { "breakTimeMissing", 10 },
            { "doubleEntry", 5 },
            { "absent", 14 }
        };

        #endregion

        #region Get Data table and punch data

        [Route("ManualAttendance/GetAllAttendance")]
        [HttpPost]
       
        public async Task<IActionResult> GetAllAttendance(AttendanceFilterViewModel filters)
        {
            int page = filters.Page;
            int pageSize = filters.PageSize;
            string department = filters.Department;
            string possibleReason = filters.PossibleReason;
            string dateRange = filters.DateRange;
            string search = filters.Search;
            string sort = filters.Sort;

            try
            {
                var imgTemFolder = GetEmployeePictureURL(true);

                //var rawData = await _manualAttendenceService.GetAllDataAsync(imgTemFolder);
                var rawDataList = await _manualAttendenceService.GetAbnormalPunchDataAsync(imgTemFolder);
                var filteredData = rawDataList.AsQueryable();
                


                var expectedKeys = new List<string> { "Duration", "InMissing", "Overtime", "OutMissing" };


                var sunna = await _manualAttendenceService.GetAbnormalTypeCountsAsync(imgTemFolder);

                // Ensure all expected keys are present
                foreach (var key in expectedKeys)
                {
                    if (!sunna.ContainsKey(key))
                    {
                        sunna[key] = 0;
                    }
                }


                // Apply filters
                if (!string.IsNullOrEmpty(department))
                {
                    filteredData = filteredData.Where(a => a.Department == department);
                }

               

                if (!string.IsNullOrEmpty(possibleReason))
                {
                    string normalizedPossibleReason = possibleReason.Replace(" ", "").ToLower();

                    filteredData = filteredData
                        .Where(a => !string.IsNullOrEmpty(a.AbnormalType) &&
                                    a.AbnormalType.Replace(" ", "").ToLower() == normalizedPossibleReason);
                }


                if (!string.IsNullOrEmpty(search))
                {
                    filteredData = filteredData.Where(a => a.EmployeeName.ToLower().Contains(search.ToLower()));
                }

                

                if (!string.IsNullOrEmpty(dateRange) && DateTime.TryParse(dateRange, out var selectedDate))
                {
                    filteredData = filteredData
                        .Where(a => DateTime.Parse(a.AttendanceDate).Date == selectedDate.Date);
                }


                // Apply sorting
                switch (sort)
                {
                    case "Latest":
                        filteredData = filteredData.OrderByDescending(a => DateTime.Parse(a.AttendanceDate));
                        break;
                    case "Oldest":
                        filteredData = filteredData.OrderBy(a => DateTime.Parse(a.AttendanceDate));
                        break;
                    case "Employee Name":
                        filteredData = filteredData.OrderBy(a => a.EmployeeName);
                        break;
                    case "Department":
                        filteredData = filteredData.OrderBy(a => a.Department);
                        break;
                    default:
                        filteredData = filteredData.OrderByDescending(a => DateTime.Parse(a.AttendanceDate));
                        break;
                }

                // Pagination
                var totalRecords = filteredData.Count();
                var pagedData = filteredData.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Json(new
                {
                    data = filteredData,
                    pagedData = pagedData,
                    totalRecords = totalRecords,
                    summary = sunna
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false
                });
            }

            
        }

        [Route("ManualAttendence/GetPunchData")]
        [HttpGet]
        public async Task<IActionResult> GetPunchData(string employeeId, string attendanceDate)
        {
            if (!int.TryParse(employeeId, out var empId) || !DateOnly.TryParse(attendanceDate, out var attDate))
            {
                return BadRequest(new { message = "Invalid employeeId or attendanceDate" });
            }

            // Step 1: Get AttendanceID
            var attendance = (await _attendanceRepository.GetAllAsync())
                                .FirstOrDefault(x => x.EmployeeID == empId && x.AttendanceDate == attDate);

           // var empData = _employeeRepository.AllActive().Where(e => e.EmployeeID == empId).Select(m => m.FirstName + " " + m.LastName + " (" + m.EmployeeCode + ")" + " (" + m.EmployeeID + ")" ).FirstOrDefault();
           
            var empData = await _attendanceRepository.AllActive().Include(o=>o.Shift).Include(i=>i.Employee)
                                .Where(x => x.EmployeeID == empId )
                                .Select(m=> new
                                {
                                    id = m.EmployeeID,
                                    name = m.Employee.FirstName + " " + m.Employee.LastName + " (" + m.Employee.EmployeeCode + ")" + " (" + m.EmployeeID + ")",
                                    shiftName = m.Shift.ShiftName,
                                    shiftTime = m.Shift.StartTime.Value.ToString("hh:mm tt") + " - " + m.Shift.EndTime.Value.ToString("hh:mm tt") + " (" + m.Shift.ShiftName + ")"
                                })
                                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return Json(new { data = new List<PunchData>() });
            }

            // Step 2: Get punch logs by AttendanceID
            var punchLogs = (await _attendanceLogRepository.GetAllAsync())
                                .Where(x => x.AttendanceID == attendance.AttendanceID)
                                .OrderBy(x => x.PunchTime)
                                .Select(x => new PunchData
                                {
                                    Time = x.PunchTime.ToString("hh:mm tt"),
                                    Label = "punch",
                                    Icon = "fas fa-fingerprint",
                                    Deletable = false
                                }).ToList();

            return Json(new { data = punchLogs , empData = empData  });
        }

        #endregion

        #region Check Cncel

        [HttpPost]
        public async Task<IActionResult> CheckCancel(string employeeId, string attendanceDate, List<PunchData> punchData)
        {
            var deletableItems = punchData.Where(p => p.Deletable).ToList();

            if (deletableItems.Any())
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        #endregion

        #region Save Data

        [HttpPost]
        public async Task<IActionResult> SavePunchData(string employeeId, string attendanceDate, List<PunchData> punchData)
        {
            var deletableItems = punchData.Where(p => p.Deletable).ToList();

            if (!deletableItems.Any())
            {
                return  Json(new { success = false , message = "no new data added" });
            }

            var attendanceDateParsed = DateOnly.TryParse(attendanceDate, out var parsedDate) ? parsedDate : DateOnly.FromDateTime(DateTime.Now);

            if (!int.TryParse(employeeId, out var empId))
            {
                return Json(new { success = false, message = "Invalid employeeId" });
            }
            if (attendanceDateParsed == default)
            {
                return Json(new { success = false, message = "Invalid attendanceDate" });
            }

            var attendanceRecord = _attendanceRepository.AllActive().FirstOrDefault(a => a.EmployeeID == empId && a.AttendanceDate == parsedDate);

            if (attendanceRecord == null)
            {
                return Json(new { success = false, message = "Attendance record not found" });
            }

            var itemData = new PunchDataListViewModel()
            {
                Punches = deletableItems
            };

            var AttendanceLog = new List<AttendanceLog>();

            foreach (var item in itemData.Punches)
            {
                if (string.IsNullOrEmpty(item.Time) || item.NotPunched)
                {
                    continue; // Skip empty or not punched items
                }
                if (!DateTime.TryParse(item.Time, out var punchTime))
                {
                    return Json(new { success = false, message = "Invalid punch time format" });
                }
                AttendanceLog.Add(new AttendanceLog
                {
                    PunchTime = punchTime,
                    SourceType = "Manual",
                    AttendanceID = attendanceRecord.AttendanceID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = itemData.CreatedBy,
                    LIP = itemData.LIP,
                    LMAC = itemData.LMAC
                   
                });
            }


            if (AttendanceLog.Count == 0)
            {
                return Json(new { success = false, message = "No valid punch data to save" });
            }

            try
            {
                // Save AttendanceLog entries
                await _attendanceLogRepository.AddRangeAsync(AttendanceLog);
               
                return Json(new { success = true, message = "Punch data saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving punch data: " + ex.Message });
            }




        }


        #endregion

        #region Save ATD


        [HttpPost]
        public IActionResult SaveAttendance(string employeeName, string attendanceDate, string actualInTime, string actualOutTime, string breakInTime, string breakOutTime)
        {

            return Json(new { success = true });
        }



        private int CalculateBiometricHits(string actualInTime, string actualOutTime, string breakInTime, string breakOutTime)
        {
            int hits = 0;
            if (!string.IsNullOrEmpty(actualInTime) && actualInTime != "Not Punched") hits++;
            if (!string.IsNullOrEmpty(actualOutTime) && actualOutTime != "Not Punched") hits++;
            if (!string.IsNullOrEmpty(breakInTime) && breakInTime != "Not Punched") hits++;
            if (!string.IsNullOrEmpty(breakOutTime) && breakOutTime != "Not Punched") hits++;
            return hits;
        }

        private string CalculatePossibleReason(List<PunchData> punchData)
        {
            if (punchData == null || punchData.Count == 0)
            {
                return "Multiple Missing";
            }
            if (punchData.Any(p => p.Label == "in time" && string.IsNullOrEmpty(p.Time)))
            {
                return "In Time Missing";
            }
            if (punchData.Any(p => p.Label == "out time" && string.IsNullOrEmpty(p.Time)))
            {
                return "Out Time Missing";
            }
            if (punchData.Any(p => p.Label == "break in" && string.IsNullOrEmpty(p.Time)))
            {
                return "Break In Missing";
            }
            if (punchData.Any(p => p.Label == "break out" && string.IsNullOrEmpty(p.Time)))
            {
                return "Break Out Missing";
            }
            if (punchData.Count(p => !p.NotPunched) > 4)
            {
                return "Over Punching";
            }
            return "Complete Record";
        }

        #endregion


        #region CheckAl 

        [HttpPost]
        [Route("ManualAttendance/MarkChecked")]
        public IActionResult MarkSelected([FromBody] List<int> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                var attendence = _attendanceRepository.All().Where(a => a.AttendanceID == item).FirstOrDefault();
                if (attendence != null)
                {
                    attendence.IsChecked = true; // Assuming IsChecked is a boolean property in Attendance model
                    _attendanceRepository.UpdateAsync(attendence);
                }

            }


            return Ok(new { success = true });
        }


        #endregion

        [Route("ManualAttendence/GetPossibleReason")]
        [HttpPost]
        public async Task<IActionResult> GetPossibleReason(string employeeId, string attendanceDate, List<PunchData> punchData)
        {
            if (!int.TryParse(employeeId, out var empId) || !DateOnly.TryParse(attendanceDate, out var attDate))
            {
                return Json(new { success = false, message = "Invalid employeeId or attendanceDate" });
            }

            try
            {
                // Fetch the full AttendanceRecord to get shift, leave, and other details
                var imgTemFolder = GetEmployeePictureURL(true);
                var allData = await _manualAttendenceService.GetAllDataAsync(imgTemFolder);
                var record = allData.FirstOrDefault(r => r.EmployeeId == empId && r.AttendanceDate == attDate.ToString("dd MMM yyyy"));

                if (record == null)
                {
                    return Json(new { success = false, message = "Attendance record not found" });
                }

                // Override the PunchData with the provided punchData from frontend
                record.PunchData = punchData.Select(p => new PunchData
                {
                    Time = p.Time,
                    Label = p.Label,
                    Icon = p.Icon,
                    Deletable = p.Deletable,
                    NotPunched = p.NotPunched
                }).ToList();


                

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
                    //var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                    //var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;

                    var firstPunchTime = ParseFlexibleTime(record.PunchData.First().Time);
                    var lastPunchTime = ParseFlexibleTime(record.PunchData.Last().Time);


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
                    //var firstPunchTime = DateTime.ParseExact(record.PunchData.First().Time, "hh:mm tt", null).TimeOfDay;
                    //var lastPunchTime = DateTime.ParseExact(record.PunchData.Last().Time, "hh:mm tt", null).TimeOfDay;

                    var firstPunchTime = ParseFlexibleTime(record.PunchData.First().Time);
                    var lastPunchTime = ParseFlexibleTime(record.PunchData.Last().Time);

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


                // Return the result
                if (reasons.Any())
                {
                    return Json(new
                    {
                        success = true,
                        possibleReason = string.Join(", ", reasons),
                        abnormalType = type
                    });
                }

                return Json(new { success = true, possibleReason = "Complete Record", abnormalType = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error evaluating punch data: {ex.Message}" });
            }
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

        private static TimeSpan ParseFlexibleTime(string timeString)
        {
            // Try 12-hour format first
            if (DateTime.TryParseExact(timeString, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt12))
                return dt12.TimeOfDay;

            // Then try 24-hour format
            if (DateTime.TryParseExact(timeString, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt24))
                return dt24.TimeOfDay;

            // Fallback: try general parsing or throw
            if (DateTime.TryParse(timeString, out var dtGeneric))
                return dtGeneric.TimeOfDay;

            throw new FormatException($"Unrecognized time format: {timeString}");
        }


        #region Maula Atend 

        [HttpGet]
        public IActionResult GetShiftInfo(string employeeId, string attendanceDate)
        {

            if (!int.TryParse(employeeId, out var empId) || !DateOnly.TryParse(attendanceDate, out var attDate))
            {
                return BadRequest(new { message = "Invalid employeeId or attendanceDate" });
            }

            var shift =  _attendanceRepository.AllActive().Include(o => o.Shift).Include(i => i.Employee)
                    .Where(x => x.EmployeeID == empId)
                    .Select(m => new
                    {
                        id = m.EmployeeID,
                        shiftTime = m.Shift.StartTime.Value.ToString("HH:mm") + " - " + m.Shift.EndTime.Value.ToString("HH:mm") + " (" + m.Shift.ShiftName + ")",
                        shiftInTime = m.Shift.StartTime.Value.ToString("HH:mm") ,
                        shiftOutTime =  m.Shift.EndTime.Value.ToString("HH:mm") ,
                        breakTime = m.Shift.MealBreakTime
                    })
                    .FirstOrDefault();

            
          

            if (shift != null)
            {
                return Json(new
                {
                    success = true,
                    shiftName = shift.shiftTime,
                    inTime = shift.shiftInTime,
                    outTime = shift.shiftOutTime,
                    breakTime = shift.breakTime
                });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult SaveManualAttendance(ManualAttendanceViewModel model)
        {
            

            var result = _manualAttendenceService.SaveManualAttendance(model).Result;
            return Ok(result);
            
            
        }


        #endregion

    }
}
