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
using static GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster.OfficeDayRosterService;

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
    }
}
