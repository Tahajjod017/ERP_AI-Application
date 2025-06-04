using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public class AssignDefaultShiftService : AppService<DefaultShifts>, IAssignDefaultShiftService
    {
        private readonly IGenericRepository<DefaultShifts> _genericRepository;

        public AssignDefaultShiftService(IGenericRepository<DefaultShifts> genericRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public Task<bool> AddAsync(AssignDefaultShiftSetupVM model)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationService<Shifts, ShiftsSetupVM>.PaginationResult<ShiftsSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            throw new NotImplementedException();
        }

        public Task<AssignDefaultShiftSetupVM> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CommonSelectVM> GetCompanies()
        {
            throw new NotImplementedException();
        }

        public Task<CommonSelectVM> GetDepartments(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonSelectVM> GetEmployees(int companyId, int departmentId)
        {
            throw new NotImplementedException();
        }

        public Task<AssignDefaultShiftSetupVM> SoftDeleteAsync(DeleteRequestVM model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(AssignDefaultShiftSetupVM model)
        {
            throw new NotImplementedException();
        }
    }
}
