using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.MonthlyReports
{
    public interface IMonthlyReportService
    {

        //Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetIndividualAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        //                       string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, int? month = null, int? departmentId = null);
        //Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetAllEmployee(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        //                       string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, int? month = null, int? departmentId = null);

        //Task<IActionResult> GetMonthlyAttendanceReport(int? departmentId, int? organizationId, int? employeeId, string monthyear);
        Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetMonthlyAttendanceReport(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                            string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, List<int>? departmentIds = null, List<int>? employeeIds = null, string? monthyear=null , int? employeeId = null);
        Task<MonthlyAttendanceCalendarVM> GetMonthlyAttendanceCalendarAsync(int? organizationId, int? departmentId,
         int employeeId,
         string monthYear);
        // Task<byte[]> GenaratePDF(int id);
        Task<byte[]> GenerateJobCardPdfAsync(int? organizationId, int? departmentId, int employeeId, string monthYear);
        //Task<List<SelectListItem>> GetOrganizationsAsync();
        //Task<List<SelectListItem>> GetMonthAsync();
        //Task<List<SelectListItem>> GetDepartmentAsync();

    }
}
