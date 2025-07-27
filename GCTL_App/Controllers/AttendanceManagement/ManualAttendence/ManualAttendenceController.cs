using System.ComponentModel.DataAnnotations;
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
            SetSmartPageCode(113000);

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

        #region Get Data 

        [Route("ManualAttendance/GetAllAttendance")]
        [HttpPost]
       // public async Task<IActionResult> GetAllAttendance(int page, int pageSize, string department, string possibleReason, string dateRange, string search, string sort)
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



                // Apply filters
                if (!string.IsNullOrEmpty(department))
                {
                    filteredData = filteredData.Where(a => a.Department == department);
                }

                //if (!string.IsNullOrEmpty(possibleReason))
                //{
                //    filteredData = filteredData.Where(a => a.PossibleReason == possibleReason);
                //}

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

                //if (!string.IsNullOrEmpty(dateRange))
                //{
                //    var dates = dateRange.Split(" to ");
                //    if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
                //    {
                //        filteredData = filteredData.Where(a => DateTime.Parse(a.AttendanceDate) >= startDate && DateTime.Parse(a.AttendanceDate) <= endDate);
                //    }
                //}

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
                    summary = _summary
                });
            }
            catch (Exception)
            {

                throw;
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
                    CreatedBy = itemData.CreatedBy
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

        #endregion

    }
}
