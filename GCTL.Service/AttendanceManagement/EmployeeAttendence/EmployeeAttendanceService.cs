using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AdminSettingsVM;
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

namespace GCTL.Service.AttendanceManagement.EmployeeAttendence
{
    public class EmployeeAttendanceService:AppService<Attendance>, IEmployeeAttendanceReport
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Attendance> _genericRepository;

        public EmployeeAttendanceService(IUserInfoService userInfoService, IGenericRepository<Attendance> genericRepository):base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
        }

        public async Task<PaginationService<Attendance, EmployeeAttendenceVM>.PaginationResult<EmployeeAttendenceVM>> GetAllAsync(
                               int pageNumber = 1,
                               int pageSize = 5,
                               string searchTerm = "",
                               string sortColumn = "OrganizationID",
                               string sortOrder = "desc",
                               int? organizationID = null,
                               int? employeeId =null)
        {
            var query = _genericRepository.All()
                .AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.Status)
                .Include(x => x.Shift)
                .Where(x => x.DeletedAt == null);

            //if (organizationID.HasValue && organizationID.Value > 0)
            //{
            //    query = query.Where(x => x.WeekendSetting.Organization.OrganizationID == organizationID.Value);
            //}

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(x => x.EmployeeID == employeeId.Value);
            }

            var result = await PaginationService<Attendance, EmployeeAttendenceVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => x => EF.Functions.Like(x.Status.StatusName, $"%{term}%")
                          ,
                x => new EmployeeAttendenceVM
                {
                    AttendanceID = x.AttendanceID,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.Employee?.FirstName + " " + x.Employee?.LastName ?? "-",
                    ShiftID = x.ShiftID,
                    ShiftName = x.Shift?.ShiftName ?? "-",
                    StatusID = x.StatusID,
                    StatusName = x.Status?.StatusName ?? "-",
                    AttendanceDate = x.AttendanceDate.ToString("yyyy-MM-dd") ?? "-",
                    CheckInTime = x.CheckInTime.HasValue ? x.CheckInTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    CheckOutTime = x.CheckOutTime.HasValue ? x.CheckOutTime.Value.ToString("HH:mm") : "-", // Fix for CS0029
                    LateHour = x.LateHour.HasValue ? x.LateHour.Value.ToString("F2") : "-",
                    EarlyHour = x.EarlyHour.HasValue ? x.EarlyHour.Value.ToString("F2") : "-",
                    OvertimeHour = x.OvertimeHour.HasValue ? x.OvertimeHour.Value.ToString("F2") : "-",
                    WorkingHours = "-",
                    Break = "-",

                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy
                });

            return result;
        }

    }
}
