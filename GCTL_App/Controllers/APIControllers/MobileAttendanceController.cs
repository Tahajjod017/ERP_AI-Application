using GCTL.Core.Repository;
using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.ManualAttendence;
using GCTL.Service.AttendanceManagement.ScheduleManagement.Attendances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                await _attendanceService.AttendanceFromApps(request);

                return Ok(new { isSuccess = true, message = "Punch data saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Error saving punch data: {ex.Message}" });
            }
        }
        #endregion
    }
}
