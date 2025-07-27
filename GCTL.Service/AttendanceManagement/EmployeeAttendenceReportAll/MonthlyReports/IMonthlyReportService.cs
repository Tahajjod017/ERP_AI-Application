using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
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

        Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetIndividualAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                               string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, int? month = null, int? departmentId = null);
        Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetAllEmployee(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                               string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null, int? month = null, int? departmentId = null);

        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetMonthAsync();
        Task<List<SelectListItem>> GetDepartmentAsync();

    }
}
