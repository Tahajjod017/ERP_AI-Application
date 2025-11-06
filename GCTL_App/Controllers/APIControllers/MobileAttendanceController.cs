using GCTL.Core.Repository;
using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.DevTools.V134.CSS;

namespace GCTL_App.Controllers.APIControllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "ApiPolicy")]
    [ApiController]
    public class MobileAttendanceController : ControllerBase
    {
        #region Services
        private readonly IAppsAttendanceService _attendanceService;

        public MobileAttendanceController(
            IAppsAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }
        #endregion


        #region Test API
        //[HttpGet("GetMobile")]
        //public IActionResult GetMobile()
        //{
        //    return Ok("✅ Mobile Attendance API is running and secured with JWT.");
        //}
        #endregion


        #region SavePunchDataFromApps
        [HttpPost("SavePunchDataFromApps")]
        public async Task<IActionResult> SavePunchDataFromApps([FromBody] PunchDataRequestVM request)
        {
            try
            {
                var result = await _attendanceService.AttendanceFromApps(request);

                if (result == null)
                    return StatusCode(500, new { isSuccess = false, message = "No data returned from stored procedure." });

                return Ok(new
                {
                    inTime = result.InTime,
                    outTime = result.OutTime,
                    imageUrl = "",
                    message = "Punch data saved successfully",
                    attendenceList = result.AttendenceListVMs.Select(a => new
                    {
                        slno = a.SlNo,
                        attendenceType = a.AttendenceType,
                        attDateANDTime = a.PunchTime.HasValue ? a.PunchTime.Value.ToString("dd MMM hh:mm tt") : ""
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Error saving punch data: {ex.Message}" });
            }
        }

        #region Test API
        //https://localhost:7086/swagger/

        //{
        //  "username": "shagormohammad002@gmail.com",
        //  "password": "R@kib123%"
        //}

        //{
        //  "employeeId": 27,
        //  "checkInTime": "2025-09-16T06:30:51"
        //  "deviceInfo": "Android",
        //  "sourceType": "Apps",
        //}

        #endregion

        #endregion


        #region GetTodaysMovement
        [HttpGet("GetTodaysMovement")]
        public async Task<IActionResult> GetTodaysMovement(int empId)
        {
            try
            {
                var result = await _attendanceService.GetTodaysMovement(empId);

                if (result == null)
                    return StatusCode(500, new { isSuccess = false, message = "No data returned from stored procedure." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Error getting punch data: {ex.Message}" });
            }
        }
        #endregion
    }
}
