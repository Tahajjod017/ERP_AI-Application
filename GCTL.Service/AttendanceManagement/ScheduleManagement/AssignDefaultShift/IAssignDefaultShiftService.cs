using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public interface IAssignDefaultShiftService
    {
        #region CRUD
        Task<bool> AddAsync(AssignDefaultShiftSetupVM model);
        Task<bool> UpdateAsync(AssignDefaultShiftSetupVM model);
        Task<AssignDefaultShiftSetupVM> SoftDeleteAsync(DeleteRequestVM model);
        Task<AssignDefaultShiftSetupVM> GetByIdAsync(int id);
        Task<PaginationService<Shifts, ShiftsSetupVM>.PaginationResult<ShiftsSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null);
        #endregion


        #region Others
        Task<CommonSelectVM> GetCompanies();
        Task<CommonSelectVM> GetDepartments(int companyId);
        Task<CommonSelectVM> GetEmployees(int companyId, int departmentId);
        #endregion
    }
}
