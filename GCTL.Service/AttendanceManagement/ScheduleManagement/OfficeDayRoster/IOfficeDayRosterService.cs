using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public interface IOfficeDayRosterService
    {
        #region CRUD
        Task<bool> AddAsync(RosterInOfficeDaysSetupVM model);
        Task<bool> UpdateAsync(RosterInOfficeDaysSetupVM model);
        Task<bool> UpdateEmpShiftAsync(RosterInOfficeDaysSetupVM model);
        Task<RosterInOfficeDaysSetupVM> SoftDeleteAsync(DeleteRequestVM model);
        Task<RosterInOfficeDaysSetupVM> GetByIdAsync(int id);
        Task<PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.PaginationResult<RosterInOfficeDaysSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int? organizationID = null);
        #endregion


        #region Others
        Task<List<CommonSelectVM>> GetCompanies();
        Task<List<CommonSelectVM>> GetDepartments();
        Task<List<RosterInOfficeDaysSetupVM>> GetGroupedEmployees();
        Task<List<CommonSelectVM>> GetShift();
        Task<List<RosterInOfficeDaysSetupVM>> GetDepartmentByCompany(int id);
        Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByCompany(int id);
        Task<List<RosterInOfficeDaysSetupVM>> GetShiftByCompany(int id);
        Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByDepartment(List<int> departmentIds);
        #endregion
    }
}
