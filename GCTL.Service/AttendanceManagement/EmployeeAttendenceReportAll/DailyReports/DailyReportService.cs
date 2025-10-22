using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.AttendenceReportAlls;
using GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.EmployeeAttendenceReportAll.DailyReports
{
    public class DailyReportService :AppService<Attendance>, IDailyReportService
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;
        private readonly IGenericRepository<Shifts> _genericRepositoryShift;
        private readonly IGenericRepository<EmployeeOfficeInfo> _genericEmployeeOfficeInfo;
        private readonly IGenericRepository<AttendanceLog> _genericAttendanceLog;

        public DailyReportService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository, IGenericRepository<EmployeeOfficeInfo> genericEmployeeOfficeInfo) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _genericEmployeeOfficeInfo = genericEmployeeOfficeInfo;
        } 
        public async Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetAllEmployee(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                              string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                        .AsNoTracking()
                        .Include(x => x.Employee)
                        .Include(x => x.Employee.EmployeeOfficeInfoCreatedByNavigation)
                        .Include(x => x.Status)
                        .Include(x => x.Shift)
                        .Where(x => x.DeletedAt == null);

            var employeeDepartments = await _genericEmployeeOfficeInfo.All()
                                         .Include(e => e.Department) // Make sure the Department is being properly loaded
                                         .Where(e => e.DeletedAt == null)
                                         .Select(e => new
                                         {
                                             EmployeeID = e.EmployeeID,
                                             // Null check for the Department to avoid any exceptions
                                             DepartmentName = e.Department != null && e.Department.DepartmentName != null
                                                              ? e.Department.DepartmentName
                                                              : "-"
                                         })
                                         .ToListAsync();


            //if (organizationID.HasValue && organizationID.Value > 0)
            //{
            //    query = query.Where(x => x.WeekendSetting.Organization.OrganizationID == organizationID.Value);
            //}



            var result = await PaginationService<Attendance, AttendanceEmployeeReportVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%"),

                x => new AttendanceEmployeeReportVM
                {

                    AttendanceID = x.AttendanceID,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    JobTitle = employeeDepartments.FirstOrDefault(e => e.EmployeeID == x.EmployeeID)?.DepartmentName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? x.CheckInTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    CheckOutTime = x.CheckOutTime.HasValue ? x.CheckOutTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    //LateHour = x.LateTimeMinutes.HasValue ? x.LateTimeMinutes.Value.ToString("F2") : "-",
                    LateHour = x.LateTimeMinutes.HasValue ? (x.LateTimeMinutes.Value).ToString("H") : "-",
                    //EarlyHour = x.EarlyTimeMinutes.HasValue ? x.EarlyTimeMinutes.Value.ToString("F2") : "-",
                    EarlyHour = x.EarlyTimeMinutes.HasValue ? (x.EarlyTimeMinutes.Value).ToString("H") : "-",
                    OvertimeHour = x.OvertimeMinutes.HasValue ? x.OvertimeMinutes.Value.ToString("H") : "-",
                    WorkingHours = "-",
                    Break = "-",

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

        public async Task<PaginationService<Attendance, AttendanceEmployeeReportVM>.PaginationResult<AttendanceEmployeeReportVM>> GetIndividualEmployee(int employeeId, int pageNumber = 1, int pageSize = 5, string searchTerm = "",
                      string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All()
                        .AsNoTracking()
                        .Include(x => x.Employee)
                        .Include(x => x.Employee.EmployeeOfficeInfoCreatedByNavigation)
                        .Include(x => x.Status)
                        .Include(x => x.Shift)
                        .Where(x => x.DeletedAt == null && x.EmployeeID == employeeId); // Filter by EmployeeID

            var employeeDepartments = await _genericEmployeeOfficeInfo.All()
                                         .Include(e => e.Department) // Ensure the Department is loaded
                                         .Where(e => e.DeletedAt == null)
                                         .Select(e => new
                                         {
                                             EmployeeID = e.EmployeeID,
                                             DepartmentName = e.Department != null && e.Department.DepartmentName != null
                                                              ? e.Department.DepartmentName
                                                              : "-"
                                         })
                                         .ToListAsync();


            var result = await PaginationService<Attendance, AttendanceEmployeeReportVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%"),

                x => new AttendanceEmployeeReportVM
                {
                    AttendanceID = x.AttendanceID,

                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    JobTitle = employeeDepartments.FirstOrDefault(e => e.EmployeeID == x.EmployeeID)?.DepartmentName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? x.CheckInTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    CheckOutTime = x.CheckOutTime.HasValue ? x.CheckOutTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                                                                                                           //LateHour = x.LateTimeMinutes.HasValue ? x.LateTimeMinutes.Value.ToString("F2") : "-",
                    LateHour = x.LateTimeMinutes.HasValue ? (x.LateTimeMinutes.Value).ToString("H") : "-",
                    //EarlyHour = x.EarlyHour.HasValue ? x.EarlyHour.Value.ToString("F2") : "-",
                    EarlyHour = x.EarlyTimeMinutes.HasValue ? (x.EarlyTimeMinutes.Value).ToString("H") : "-",
                    OvertimeHour = x.OvertimeMinutes.HasValue ? x.OvertimeMinutes.Value.ToString("H") : "-",
                    WorkingHours = "-",
                    Break = "-",

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

        public async Task<AttendanceSummaryDto> GetSummaryAll()
        {
            var today = DateTime.Today;

            var attendancesToday = _genericRepository.All()
                .Where(x => x.DeletedAt == null && x.AttendanceDate == DateOnly.FromDateTime(today)); // Fix: Compare DateOnly with DateOnly

            var totalLate = await attendancesToday
                .Where(x => x.LateTimeMinutes > 0)
                .CountAsync();

            var totalLeave = await attendancesToday
                .Where(x => x.EarlyTimeMinutes > 0)
                .CountAsync();

            var totalAbsent = await attendancesToday
                .Where(x => x.OvertimeMinutes > 0)
                .CountAsync();

            // Present = total who punched in, excluding those who were late, left early, or had overtime
            var totalPresent = await attendancesToday
                .Where(x => x.LateTimeMinutes == 0 && x.EarlyTimeMinutes == 0 && x.OvertimeMinutes == 0)
                .CountAsync();

            var total = totalPresent + totalLate + totalLeave + totalAbsent;
            total = total == 0 ? 1 : total;

            return new AttendanceSummaryDto
            {
                Present = totalPresent,
                LatePresent = totalLate,
                Leave = totalLeave,
                Absent = totalAbsent,

                PresentPercent = (int)Math.Round((double)totalPresent / total * 100),
                LatePresentPercent = (int)Math.Round((double)totalLate / total * 100),
                LeavePercent = (int)Math.Round((double)totalLeave / total * 100),
                AbsentPercent = (int)Math.Round((double)totalAbsent / total * 100)
            };
        }
    }
}
