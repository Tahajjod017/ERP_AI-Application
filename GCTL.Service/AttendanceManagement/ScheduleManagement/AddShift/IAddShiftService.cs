using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
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
        Task<bool> UpdateAsync(ShiftUpdateSetupVM model);
        Task<ShiftsSetupVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<GetByIdShiftVM> GetByIdAsync(int id);
        Task<PaginationService<Shifts, ShiftsListVM>.PaginationResult<ShiftsListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null);
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(int id, string name);
        #endregion
    }
}
