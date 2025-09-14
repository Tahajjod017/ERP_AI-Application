using GCTL.Core.Repository;
using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ManualAttendence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.APIControllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class MobileAttendanceController : ControllerBase
    {
        private readonly IGenericRepository<Attendance> _attendanceRepository;
        private readonly IGenericRepository<AttendanceLog> _attendanceLogRepository;

        public MobileAttendanceController(
            IGenericRepository<Attendance> attendanceRepository, 
            IGenericRepository<AttendanceLog> attendanceLogRepository)
        {
            _attendanceRepository = attendanceRepository;
            _attendanceLogRepository = attendanceLogRepository;
        }



        [HttpPost("SavePunchDataFromMobile")]
        public async Task<IActionResult> SavePunchDataFromMobile([FromBody] PunchDataRequestVM request)
        {
            if (request.PunchDataVMs == null || !request.PunchDataVMs.Any(p => p.Deletable))
            {
                return BadRequest(new { success = false, message = "No new data added" });
            }

            var deletableItems = request.PunchDataVMs.Where(p => p.Deletable).ToList();

            var attendanceDateParsed = DateOnly.TryParse(request.AttendanceDate, out var parsedDate)
                ? parsedDate
                : DateOnly.FromDateTime(DateTime.Now);

            if (!int.TryParse(request.EmployeeId, out var empId))
            {
                return BadRequest(new { success = false, message = "Invalid employeeId" });
            }

            if (attendanceDateParsed == default)
            {
                return BadRequest(new { success = false, message = "Invalid attendanceDate" });
            }

            var attendanceRecord = _attendanceRepository
                .AllActive()
                .FirstOrDefault(a => a.EmployeeID == empId && a.AttendanceDate == parsedDate);

            if (attendanceRecord == null)
            {
                return NotFound(new { success = false, message = "Attendance record not found" });
            }

            var attendanceLog = new List<AttendanceLog>();

            foreach (var item in deletableItems)
            {
                if (string.IsNullOrEmpty(item.Time) || item.NotPunched)
                    continue;

                if (!DateTime.TryParse(item.Time, out var punchTime))
                {
                    return BadRequest(new { success = false, message = "Invalid punch time format" });
                }

                attendanceLog.Add(new AttendanceLog
                {
                    PunchTime = punchTime,
                    SourceType = "Manual",
                    AttendanceID = attendanceRecord.AttendanceID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = item.CreatedBy,
                    LIP = item.LIP,
                    LMAC = item.LMAC
                });
            }

            if (attendanceLog.Count == 0)
            {
                return BadRequest(new { success = false, message = "No valid punch data to save" });
            }

            try
            {
                await _attendanceLogRepository.AddRangeAsync(attendanceLog);

                return Ok(new { success = true, message = "Punch data saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error saving punch data: {ex.Message}" });
            }
        }
    }
}
