using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public interface IAssignSpiralPatternService
    {
        Task<bool> AddAsync(AssignSpiralPatternSetupVM model);

        Task<SeparatePaginationResult<AssignSpiralPatternListVM>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralPatternAssignListID",
            string sortOrder = "desc");

        Task<List<SpiralWeeklyPatternList>> GetAllSpiralWeeklyPatternAsync(int id);
        Task<List<SpiralBioWeeklyPatternListVM>> GetAllSpiralFortnightlyPatternAsync(int id);
        Task<List<SpiralMonthlyPatternListVM>> GetAllSpiralMonthlyPatternAsync(int id);
    }
}
