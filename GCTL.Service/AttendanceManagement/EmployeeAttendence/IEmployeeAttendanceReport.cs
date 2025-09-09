using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendence
{
    public interface IEmployeeAttendanceReport
    {
        Task<PaginationService<Attendance, EmployeeAttendenceVM>.PaginationResult<EmployeeAttendenceVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, int? employeeId = null, int? statusID = null, string? sortId = "");
        Task<EmployeeAttendenceVM> GetAttendanceDetailsAsync(int employeeId);
        Task<List<PunchActivityDto>> GetEmployeePunchActivityAsync(int userId);
        Task<double> GetTotalHoursForWeek(int employeeId, int organizationId, int? organizationBranchId);
        Task<IActionResult> CalculateWorkingHours(int attendanceId);

    }
}
