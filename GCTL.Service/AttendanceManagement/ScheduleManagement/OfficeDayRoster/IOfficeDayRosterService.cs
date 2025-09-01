using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public interface IOfficeDayRosterService
    {
        #region CRUD
        Task<bool> AddAsync(RosterInOfficeDaysSetupVM model);
        Task<bool> AddEmpShiftAsync(RosterInOfficeDayModalAddVM model);
        Task<bool> UpdateEmpShiftAsync(RosterInOfficeDayEditVM model);
        Task<RosterInOfficeDaysSetupVM> GetByIdAsync(int id);
        Task<SeparatePaginationResult<RosterInOfficeDaysListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", int daysToShow = 7, DateTime? startDate = null);
        #endregion
        Task<PaginationResult2<RosterInOfficeDaysListVM>> GetPagedEmployeesAsync(int pageNumber, int pageSize, string searchTerm, int daysToShow = 7, DateTime? startDate = null);
    }

    public class PaginationResult2<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
    }
}
