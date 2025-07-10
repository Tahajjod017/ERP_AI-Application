using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster.OfficeDayRosterService;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public interface IOfficeDayRosterService
    {
        #region CRUD
        //Task<bool> AddAsync(RosterInOfficeDaysSetupVM model);
        //Task<bool> UpdateAsync(RosterInOfficeDaysSetupVM model);
        //Task<bool> UpdateEmpShiftAsync(RosterInOfficeDaysOverrideSetupVM model);
        //Task<RosterInOfficeDaysSetupVM> SoftDeleteAsync(RosterDelVM model);
        Task<RosterInOfficeDaysSetupVM> GetByIdAsync(int id);
        Task<List<RosterInOfficeDaysSetupVM>> GetAllFromSPAsync(int pageNumber, int pageSize, string searchTerm, string sortColumn, string sortOrder, int daysToShow);
        //Task<PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.PaginationResult<RosterInOfficeDaysSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        //    string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7);
        //        Task<PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.PaginationResult<RosterInOfficeDaysSetupVM>> GetAllAsync(
        //    int pageNumber = 1,
        //    int pageSize = 5,
        //    string searchTerm = "",
        //    string sortColumn = "RosterInOfficeDayID",
        //    string sortOrder = "desc",
        //    int daysToShow = 7,
        //    DateTime? startDate = null
        //);

    //    Task<(List<RosterEmployeeGroupedVM> Data, PaginationInfo2 Pagination)> GetAllGroupedAsync(
    //int pageNumber = 1,
    //int pageSize = 5,
    //string searchTerm = "",
    //string sortColumn = "RosterInOfficeDayID",
    //string sortOrder = "desc",
    //int daysToShow = 7,
    //DateTime? startDate = null);
        #endregion


        #region Others
        Task<List<CommonSelectVM>> GetCompanies();
        Task<List<CommonSelectVM>> GetBrnach();
        Task<List<CommonSelectVM>> GetDepartments();
        Task<List<RosterInOfficeDaysSetupVM>> GetGroupedEmployees();
        Task<List<CommonSelectVM>> GetShift();
        Task<List<RosterInOfficeDaysSetupVM>> GetBranchByOrganization(int? id);
        Task<List<RosterInOfficeDaysSetupVM>> GetDepartmentByOrganization(int? id);
        Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByOrganization(int? id);
        Task<List<RosterInOfficeDaysSetupVM>> GetShiftByOrganization(int? id);
        Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByBranch(int? orgId, List<int?> ids);
        Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByDepartment(int? orgId, List<int>? departmentIds);
        #endregion
    }
}
