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
        Task<bool> UpdateEmpShiftAsync(RosterInOffDayEditVM model);
        Task<(List<RosterInOffDayListVM> Data, List<string> UniqueDates, SeparatePaginationInfo Pagination)> GetAllAsync(
            int pageNumber = 1, 
            int pageSize = 5, 
            string searchTerm = "", 
            string sortColumn = "RosterInHolyDayID", 
            string sortOrder = "desc", 
            int daysToShow = 7,
            DateTime? startDate = null);
    }
}
