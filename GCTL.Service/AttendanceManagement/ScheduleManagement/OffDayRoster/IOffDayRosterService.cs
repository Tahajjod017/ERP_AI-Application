using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public interface IOffDayRosterService
    {
        Task<bool> AddAsync(RosterInOffDaySetupVM model);
        Task<SeparatePaginationResult<RosterInOffDayListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOffDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null);
        Task<(List<RosterInOffDayListVM> rosterList, List<string> uniqueDates)> GetAll();
    }
}
