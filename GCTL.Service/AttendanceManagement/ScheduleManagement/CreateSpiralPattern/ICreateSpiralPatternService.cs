using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public interface ICreateSpiralPatternService
    {
        Task<bool> AddAsync(CreateSpiralPatternVM model);
        Task<bool> UpdateAsync(UpdateSpiralPatternVM model);
        Task<(List<SpiralWeeklyPatternList> Data, SeparatePaginationInfo Pagination)> GetAllSpiralWeeklyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralWeeklyPatternID",
            string sortOrder = "desc");
        Task<(List<SpiralBioWeeklyPatternListVM> Data, SeparatePaginationInfo Pagination)> GetAllSpiralFortnightlyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralBioWeeklyPatternID",
            string sortOrder = "desc");
        Task<(List<SpiralMonthlyPatternListVM> Data, SeparatePaginationInfo Pagination)> GetAllSpiralMonthlyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralMonthlyPatternID",
            string sortOrder = "desc");
    }
}
