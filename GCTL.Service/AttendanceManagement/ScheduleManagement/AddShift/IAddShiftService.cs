using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AddShift
{
    public interface IAddShiftService
    {
        #region CRUD
        Task<bool> AddAsync(ShiftsSetupVM model);
        Task<bool> UpdateAsync(ShiftsSetupVM model);
        Task<ShiftsSetupVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<ShiftsSetupVM> GetByIdAsync(int id);
        Task<PaginationService<Shifts, ShiftsSetupVM>.PaginationResult<ShiftsSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ShiftID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        IEnumerable<CommonSelectVM> GetOrganizations();
        #endregion
    }
}
