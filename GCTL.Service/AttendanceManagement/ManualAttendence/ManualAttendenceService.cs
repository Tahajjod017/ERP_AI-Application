using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence;
using GCTL.Data.Models;

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
            var employees = await _employeeRepository.GetAllAsync();
            var official = await _officialRepository.GetAllAsync(); // This is Attendance
            var attendances = await _attendenceRepository.GetAllAsync(); // This is Attendance
            var logs = await _attendenceLogRepository.GetAllAsync();   // AttendanceLog
            var departments = await _departmentRepository.GetAllAsync();
            var designations = await _designationRepository.GetAllAsync();
            var shifts = await _shiftRepository.GetAllAsync();
            var leaves = await _leaveRepository.GetAllAsync();

            var attendanceList = (from att in attendances

                                  join emp in employees on att.EmployeeID equals emp.EmployeeID into empGroup
                                  from emp in empGroup.DefaultIfEmpty()

                                  join shift in shifts on att.ShiftID equals shift.ShiftID into shiftGroup
                                  from shift in shiftGroup.DefaultIfEmpty()

                                  join off in official on emp.EmployeeID equals off.EmployeeID into offGroup
                                  from off in offGroup.DefaultIfEmpty()

                                  join dept in departments on off.DepartmentID equals dept.DepartmentID into deptGroup
                                  from dept in deptGroup.DefaultIfEmpty()

                                  join des in designations on off.DesignationID equals des.DesignationID into desGroup
                                  from des in desGroup.DefaultIfEmpty()

                                  join lev in leaves on att.EmployeeID equals lev.EmployeeID into levGroup
                                  from lev in levGroup.DefaultIfEmpty()


                                  select new AttendanceRecord
                                  {
                                      EmployeeId = emp.EmployeeID,
                                      Id = att.AttendanceID,
                                      EmployeeName = emp?.FirstName + emp?.LastName ?? "N/A",
                                      EmployeeRole = des?.DesignationName ?? "N/A",
                                      Department = dept?.DepartmentName ?? "N/A",
                                      EmployeeImage = imgTemFolder + emp?.EmployeeImageFileName ?? "https://placehold.co/300x200?text=Photo",
                                      AttendanceDate = att.AttendanceDate.ToString("dd MMM yyyy"),
                                      ScheduleTime = shift != null ? $"{shift.StartTime:hh\\:mm tt} - {shift.EndTime:hh\\:mm tt}" : "N/A",
                                      ActualInTime = att.CheckInTime?.ToString("hh:mm tt") ?? "Not Punched",
                                      ActualOutTime = att.CheckOutTime?.ToString("hh:mm tt") ?? "Not Punched",
                                      BreakInTime = shift.MealBreakStartTime?.ToString("hh:mm tt") ?? "Not Punched",
                                      BreakOutTime = shift.MealBreakEndTime?.ToString("hh:mm tt") ?? "Not Punched",
                                      Overtime = att.OvertimeHour > 0 ? $"{att.OvertimeHour} hrs" : "No Overtime",
                                      BiometricHits = logs.Count(x => x.AttendanceID == att.AttendanceID),
                                     
                                      PossibleReason = "",
                                      GraceTime = shift.GraceTime,
                                      MinimumWorkHour = shift.MinimumWorkingTime,
                                      isOvertimeEligible = shift.IsAllowOvertime,
                                      MinimumOverTime = shift.MinimumRequiredOvertime,
                                      MaximumOverTime = shift.MaximumAllowedOvertime,

                                      IsOnFullLeave = lev != null && lev.StatusID == 2 && lev.IsFullDay && att.AttendanceDate >= lev.FromDate && att.AttendanceDate <= lev.ToDate,

                                      IsPartialLeave = lev != null && lev.StatusID == 2 && !lev.IsFullDay && att.AttendanceDate == lev.FromDate,

                                      PartialLeaveTimeRange = lev != null && lev.StatusID == 2 && !lev.IsFullDay ? $"{lev.PartialFromTime:hh\\:mm tt} - {lev.PartialToTime:hh\\:mm tt}" : null,

                                      PunchData = logs
                                          .Where(x => x.AttendanceID == att.AttendanceID)
                                          .OrderBy(x => x.PunchTime)
                                          .Select(x => new PunchData
                                          {
                                              Time = x.PunchTime.ToString("hh:mm tt"),
                                              Label = "punch",
                                              Icon = "fas fa-fingerprint",
                                              Deletable = false
                                          }).ToList()
                                  }).ToList();

            return attendanceList;
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



    }
}
