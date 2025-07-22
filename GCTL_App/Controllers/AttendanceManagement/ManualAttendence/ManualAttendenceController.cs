using System.Runtime.InteropServices;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using GCTL.Service.AttendanceManagement.ManualAttendence;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.AttendanceManagement.ManualAttendence
{
    public class ManualAttendenceController : BaseController
    {
        private readonly IManualAttendenceService _manualAttendenceService;
        public ManualAttendenceController(ITranslateService translateService, IUserProfileService userProfileService, IManualAttendenceService manualAttendenceService) : base(translateService, userProfileService)
        {
            _manualAttendenceService = manualAttendenceService;
        }
       
        public IActionResult Index()
        {
            SetSmartPageCode(113000);

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllAttendance()
        //{
        //    await GetCurrentEmployeeIdAsync();

           
        //    var attendanceData = new[]
        //    {
        //        new {
        //            id = 1,
        //            employeeName = "Faruk Hasan",
        //            employeeRole = "Admin",
        //            department = "IT",
        //            employeeImage = "https://placehold.co/300x200?text=Placeholder",
        //            attendanceDate = "20 Jul 2025",
        //            scheduleTime = "09:00 AM - 06:00 PM",
        //            actualInTime = "09:45 AM",
        //            actualOutTime = "Not Punched",
        //            breakInTime = "01:00 PM",
        //            breakOutTime = "Not Punched",
        //            overtime = "No Overtime",
        //            biometricHits = 3,
        //            possibleReason = "Break In/Out Missing"
        //        },
        //        new {
        //            id = 2,
        //            employeeName = "Aminul Islam",
        //            employeeRole = "Manager",
        //            department = "HR",
        //            employeeImage = "https://placehold.co/300x200?text=Placeholder",
        //            attendanceDate = "19 Jul 2025",
        //            scheduleTime = "08:30 AM - 05:30 PM",
        //            actualInTime = "Not Punched",
        //            actualOutTime = "05:30 PM",
        //            breakInTime = "Not Punched",
        //            breakOutTime = "Not Punched",
        //            overtime = "No Overtime",
        //            biometricHits = 1,
        //            possibleReason = "In Time Missing"
        //        }
        //    };

        //    return Ok(attendanceData);
        //}



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

        [Route("ManualAttendance/GetAllAttendance")]
        [HttpPost]
        public IActionResult GetAllAttendance(int page, int pageSize, string department, string possibleReason, string dateRange, string search, string sort)
        {
            var filteredData = _attendanceData.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(department))
            {
                filteredData = filteredData.Where(a => a.Department == department);
            }

            if (!string.IsNullOrEmpty(possibleReason))
            {
                filteredData = filteredData.Where(a => a.PossibleReason == possibleReason);
            }

            if (!string.IsNullOrEmpty(search))
            {
                filteredData = filteredData.Where(a => a.EmployeeName.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" to ");
                if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
                {
                    filteredData = filteredData.Where(a => DateTime.Parse(a.AttendanceDate) >= startDate && DateTime.Parse(a.AttendanceDate) <= endDate);
                }
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
                summary = _summary
            });
        }

        [Route("ManualAttendence/GetPunchData")]
        [HttpGet]
        public IActionResult GetPunchData(string employeeId, string attendanceDate)
        {
            var record = _attendanceData.FirstOrDefault(a => a.Id.ToString() == employeeId && a.AttendanceDate == attendanceDate);
            return Json(new { data = record?.PunchData ?? new List<PunchData>() });
        }

        [HttpPost]
        public IActionResult SavePunchData(string employeeId, string attendanceDate, List<PunchData> punchData)
        {
            var deletableItems = punchData.Where(p => p.Deletable).ToList();

            if (!deletableItems.Any())
            {
                return Json(new { success = false , message = "no new data added" });
            }

            var record = _attendanceData.FirstOrDefault(a => a.Id.ToString() == employeeId && a.AttendanceDate == attendanceDate);
            if (record != null)
            {
                record.PunchData = punchData;
                // Update other fields (e.g., biometricHits, possibleReason) based on punchData
                record.BiometricHits = punchData.Count(p => !p.NotPunched);
                record.PossibleReason = CalculatePossibleReason(punchData);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SaveAttendance(string employeeName, string attendanceDate, string actualInTime, string actualOutTime, string breakInTime, string breakOutTime)
        {
            var record = _attendanceData.FirstOrDefault(a => a.EmployeeName == employeeName && a.AttendanceDate == attendanceDate);
            if (record != null)
            {
                record.ActualInTime = actualInTime;
                record.ActualOutTime = actualOutTime;
                record.BreakInTime = breakInTime;
                record.BreakOutTime = breakOutTime;
                record.BiometricHits = CalculateBiometricHits(actualInTime, actualOutTime, breakInTime, breakOutTime);
                record.PossibleReason = CalculatePossibleReason(record.PunchData);
            }
            else
            {
                var newRecord = new AttendanceRecord
                {
                    Id = _attendanceData.Max(a => a.Id) + 1,
                    EmployeeName = employeeName,
                    EmployeeRole = "Unknown",
                    Department = "Unknown",
                    EmployeeImage = "https://placehold.co/300x200?text=Placeholder",
                    AttendanceDate = attendanceDate,
                    ScheduleTime = "09:00 AM - 06:00 PM",
                    ActualInTime = actualInTime,
                    ActualOutTime = actualOutTime,
                    BreakInTime = breakInTime,
                    BreakOutTime = breakOutTime,
                    Overtime = "No Overtime",
                    BiometricHits = CalculateBiometricHits(actualInTime, actualOutTime, breakInTime, breakOutTime),
                    PossibleReason = CalculatePossibleReason(null),
                    PunchData = new List<PunchData>()
                };
                _attendanceData.Add(newRecord);
            }
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

    }
}
