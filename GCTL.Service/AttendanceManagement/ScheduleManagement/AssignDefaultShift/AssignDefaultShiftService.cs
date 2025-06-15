using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignDefaultShift
{
    public class AssignDefaultShiftService : AppService<DefaultShifts>, IAssignDefaultShiftService
    {
        #region Repositories
        private readonly IGenericRepository<DefaultShifts> _genericRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
        private readonly IGenericRepository<Shifts> _shiftsRepository;

        public AssignDefaultShiftService(IGenericRepository<DefaultShifts> genericRepository,
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Data.Models.Employees> employeesRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo,
            IGenericRepository<Shifts> shiftsRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _employeesRepository = employeesRepository;
            _employeeOfficeInfo = employeeOfficeInfo;
            _shiftsRepository = shiftsRepository;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(AssignDefaultShiftSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                foreach (var empId in model.EmployeeIDs)
                {
                    var employee = (await _employeeOfficeInfo.FindAsync(x => x.EmployeeID == empId)).FirstOrDefault();

                    if (employee == null || employee.DepartmentID == null)
                        continue;

                    DefaultShifts entity = new DefaultShifts();
                    entity.ShiftID = model.ShiftID;
                    entity.OrganizationID = employee.OrganizationID;
                    entity.DepartmentID = employee.DepartmentID;
                    entity.EmployeeID = empId;

                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy;
                    entity.CreatedAt = DateTime.Now;

                    await _genericRepository.AddAsync(entity);
                }

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch(Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateAsync(AssignDefaultShiftSetupVM model)
        {
            var shift = await _genericRepository.GetByIdAsync(model.ShiftID);
            throw new NotImplementedException();
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<DefaultShifts, AssignDefaultShiftSetupVM>.PaginationResult<AssignDefaultShiftSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DefaultShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            var query = _genericRepository.All().AsNoTracking().Include(x => x.Shift).Include(x => x.Organization).Include(x => x.Department).Include(x => x.Employee).Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "DefaultShiftID" => sortOrder == "desc" ? query.OrderByDescending(x => x.DefaultShiftID) : query.OrderBy(x => x.DefaultShiftID),
                    "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Shift.ShiftName) : query.OrderBy(x => x.Shift.ShiftName),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
                    "DepartmentName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Department.DepartmentName) : query.OrderBy(x => x.Department.DepartmentName),
                    "EmployeeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Employee.FirstName) : query.OrderBy(x => x.Employee.FirstName),
                    _ => query.OrderBy(x => x.ShiftID)
                };
            }

            var result = await PaginationService<DefaultShifts, AssignDefaultShiftSetupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.Shift.ShiftName, $"%{term}%") || EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") ||
                EF.Functions.Like(x.Department.DepartmentName, $"%{term}%") || EF.Functions.Like(x.Employee.FirstName, $"%{term}%"), 
                x => new AssignDefaultShiftSetupVM
                {
                    OrganizationName = x.Organization.OrganizationName ?? "-",
                    DepartmentName = x.Department.DepartmentName ?? "-",
                    EmployeeName = $"{x.Employee.FirstName} {x.Employee.LastName} ({x.Employee.EmployeeCode})",
                    ShiftName = x.Shift.ShiftName ?? "-",
                });

            return result;
        }
        #endregion


        #region GetByIdAsync
        public async Task<AssignDefaultShiftSetupVM> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id); 
            var defaultShift = entity as DefaultShifts;

            if (defaultShift == null)
                return null;

            return new AssignDefaultShiftSetupVM
            {
                DefaultShiftID = defaultShift.DefaultShiftID,
                ShiftID = defaultShift.ShiftID,
                OrganizationID = defaultShift.OrganizationID,
                DepartmentID = defaultShift.DepartmentID,
                EmployeeID = defaultShift.EmployeeID
            };
        }
        #endregion


        #region GetCompanies
        public Task<List<CommonSelectVM>> GetCompanies()
        {
            var data = _organizationRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.OrganizationID,
                    Name = x.OrganizationName
                }).ToListAsync();
            return data;
        }
        #endregion


        #region GetDepartments
        public async Task<List<CommonSelectVM>> GetDepartments()
        {
            var data = await _departmentRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.DepartmentID,
                    Name = x.DepartmentName
                }).ToListAsync();
            return data;
        }
        #endregion


        #region GetGroupedEmployees
        public async Task<List<AssignDefaultShiftSetupVM>> GetGroupedEmployees()
        {
            var data = await (from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                              join emp in _employeesRepository.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join org in _organizationRepository.AllActive() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                              from org in orgGroup.DefaultIfEmpty()

                              join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new AssignDefaultShiftSetupVM
                              {
                                  EmployeeID = empOi.EmployeeID ?? 0,
                                  EmployeeName = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  DepartmentName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion


        #region GetFilteredEmployees
        public async Task<List<AssignDefaultShiftSetupVM>> GetFilteredEmployees(List<int> organizationIds, List<int> departmentIds)
        {
            var query = from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()
                        join emp in _employeesRepository.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()
                        join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()
                        select new
                        {
                            empOi.EmployeeID,
                            emp.FirstName,
                            emp.LastName,
                            emp.EmployeeCode,
                            dep.DepartmentName,
                            empOi.OrganizationID,
                            empOi.DepartmentID
                        };

            if (organizationIds?.Any() == true)
                query = query.Where(x => organizationIds.Contains(x.OrganizationID ?? 0));

            if (departmentIds?.Any() == true)
                query = query.Where(x => departmentIds.Contains(x.DepartmentID ?? 0));

            return await query
                .Select(x => new AssignDefaultShiftSetupVM
                {
                    EmployeeID = x.EmployeeID ?? 0,
                    EmployeeName = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                    DepartmentName = x.DepartmentName
                }).ToListAsync();
        }
        #endregion


        #region GetShift
        public async Task<List<CommonSelectVM>> GetShift()
        {
            var data = await _shiftsRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.ShiftID,
                    Name = $"{x.ShiftName} ({x.StartTime} - {x.EndTime})"
                }).ToListAsync();
            return data;
        }
        #endregion


        #region SoftDeleteAsync
        public Task<AssignDefaultShiftSetupVM> SoftDeleteAsync(DeleteRequestVM model)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
