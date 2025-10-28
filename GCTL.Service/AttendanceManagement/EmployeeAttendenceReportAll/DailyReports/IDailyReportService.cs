using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports
{
    public interface IDailyReportService
    {
        Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetAllEmployee(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                             string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetIndividualEmployee(int employeeId, int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc" , int? organizationID = null);
        Task<AttendanceSummaryDto> GetSummaryAll();
        Task<List<CommonSelectVM>> GetDepartmentByOrgId(int? orgId);
        Task<List<CommonSelectVM>> GetEmployeeByDepId(int? depId);

    }
}
