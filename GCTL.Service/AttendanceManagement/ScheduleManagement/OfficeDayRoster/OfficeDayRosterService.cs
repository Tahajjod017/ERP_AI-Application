using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    public class OfficeDayRosterService : AppService<RosterInOfficeDays>, IOfficeDayRosterService
    {
        #region Repositories
        private readonly IGenericRepository<RosterInOfficeDays> _genericRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
        private readonly IGenericRepository<Shifts> _shiftsRepository;

        public OfficeDayRosterService(IGenericRepository<RosterInOfficeDays> genericRepository, 
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
        public async Task<bool> AddAsync(RosterInOfficeDaysSetupVM model)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateAsync(RosterInOfficeDaysSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.RosterInOfficeDayID);
                if (entity == null)
                {
                    return false;
                }

                entity.OrganizationID = model.OrganizationID;
                entity.DepartmentID = model.DepartmentIDs.FirstOrDefault();
                entity.EmployeeID = model.EmployeeIDs.FirstOrDefault();
                entity.ShiftID = model.ShiftID;

                await _genericRepository.UpdateAsync(entity);
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateEmpShiftAsync(RosterInOfficeDaysSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.RosterInOfficeDayID);
                if (entity == null)
                {
                    return false;
                }

                entity.OrganizationID = model.OrganizationID;
                entity.DepartmentID = model.DepartmentIDs.FirstOrDefault();
                entity.EmployeeID = model.EmployeeIDs.FirstOrDefault();
                entity.ShiftID = model.ShiftID;

                await _genericRepository.UpdateAsync(entity);
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.PaginationResult<RosterInOfficeDaysSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int? organizationID = null)
        {
            //var query = _genericRepository.All().AsNoTracking().Include(x => x.Shift).Include(x => x.Organization).Include(x => x.Department).Include(x => x.Employee).Where(x => x.DeletedAt == null);
            var query = _genericRepository.All().AsNoTracking().Include(x => x.Shift).Include(x => x.Organization).Include(x => x.Employee).Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "RosterInOfficeDayID" => sortOrder == "desc" ? query.OrderByDescending(x => x.RosterInOfficeDayID) : query.OrderBy(x => x.RosterInOfficeDayID),
                    "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Shift.ShiftName) : query.OrderBy(x => x.Shift.ShiftName),
                    "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
                    //"DepartmentName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Department.DepartmentName) : query.OrderBy(x => x.Department.DepartmentName),
                    "EmployeeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Employee.FirstName) : query.OrderBy(x => x.Employee.FirstName),
                    _ => query.OrderBy(x => x.ShiftID)
                };
            }

            var result = await PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.Shift.ShiftName, $"%{term}%") || EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%") ||
                //EF.Functions.Like(x.Department.DepartmentName, $"%{term}%") || 
                EF.Functions.Like(x.Employee.FirstName, $"%{term}%"),
                x => new RosterInOfficeDaysSetupVM
                {
                    RosterInOfficeDayID = x.RosterInOfficeDayID,
                    OrganizationName = x.Organization.OrganizationName ?? "-",
                    //DepartmentName = x.Department.DepartmentName ?? "-",
                    EmployeeName = $"{x.Employee.FirstName} {x.Employee.LastName} ({x.Employee.EmployeeCode})",
                    //ShiftName = x.Shift.ShiftName ?? "-",
                });

            return result;
        }
        #endregion


        #region GetByIdAsync
        public async Task<RosterInOfficeDaysSetupVM> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            var defaultShift = entity as RosterInOfficeDays;

            if (defaultShift == null)
                return null;

            return new RosterInOfficeDaysSetupVM
            {
                RosterInOfficeDayID = defaultShift.RosterInOfficeDayID,
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
        public async Task<List<RosterInOfficeDaysSetupVM>> GetGroupedEmployees()
        {
            var data = await (from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                              join emp in _employeesRepository.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join org in _organizationRepository.AllActive() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                              from org in orgGroup.DefaultIfEmpty()

                              join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new RosterInOfficeDaysSetupVM
                              {
                                  EmployeeID = empOi.EmployeeID ?? 0,
                                  EmployeeName = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  DepartmentName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion


        #region GetFilteredEmployees
        public async Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByDepartment(int? orgId, List<int>? departmentIds)
        {
            var query = from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()
                        
                        join emp in _employeesRepository.AllActive().AsNoTracking() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()

                        join dep in _departmentRepository.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
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

            if (departmentIds?.Any() == true)
                query = query.Where(x => x.OrganizationID == orgId && departmentIds.Contains(x.DepartmentID ?? 0));

            return await query
                .Select(x => new RosterInOfficeDaysSetupVM
                {
                    EmployeeID = x.EmployeeID ?? 0,
                    EmployeeName = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                    DepartmentName = x.DepartmentName
                }).AsNoTracking().ToListAsync();
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
        public Task<RosterInOfficeDaysSetupVM> SoftDeleteAsync(DeleteRequestVM model)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region GetDepartmentByOrganization
        public async Task<List<RosterInOfficeDaysSetupVM>> GetDepartmentByOrganization(int? id)
        {
            var query = from eoi in _employeeOfficeInfo.AllActive().AsNoTracking()

                        join dep in _departmentRepository.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()

                        select new { eoi, dep };

            if (id.HasValue && id.Value != 0)
            {
                query = query.Where(x => x.eoi.OrganizationID == id.Value);
            }

            var result = await query
                .Select(x => new RosterInOfficeDaysSetupVM
                {
                    DepartmentID = x.eoi.DepartmentID ?? 0,
                    DepartmentName = x.dep.DepartmentName
                }).Distinct().AsNoTracking().ToListAsync();

            return result;
        }

        #endregion


        #region GetEmployeeByOrganization
        public async Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByOrganization(int? id)
        {
            var query = from eoi in _employeeOfficeInfo.AllActive().AsNoTracking()

                        join emp in _employeesRepository.AllActive().AsNoTracking() on eoi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()

                        join dep in _departmentRepository.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()

                        select new { eoi, emp, dep };

            if(id.HasValue && id.Value != 0)
            {
                query = query.Where(x => x.eoi.OrganizationID == id.Value);
            }

            var result = await query
                .Select(x => new RosterInOfficeDaysSetupVM
                {
                    EmployeeID = x.eoi.EmployeeID,
                    EmployeeName = $"{x.emp.FirstName} {x.emp.LastName} ({x.emp.EmployeeCode})",
                    DepartmentName = x.dep.DepartmentName
                }).Distinct().AsNoTracking().ToListAsync();

            return result;
        }
        #endregion


        #region GetShiftByCompany
        public async Task<List<RosterInOfficeDaysSetupVM>> GetShiftByOrganization(int? id)
        {
            var query = from sft in _shiftsRepository.AllActive().AsNoTracking()

                        join org in _organizationRepository.AllActive().AsNoTracking() on sft.OrganizationID equals org.OrganizationID into orgGroup
                        from org in orgGroup.DefaultIfEmpty()

                        select new { sft, org };

            if(id.HasValue && id.Value != 0)
            {
                query = query.Where(x => x.sft.OrganizationID == id.Value);
            }

            var result = await query
                .Select(x => new RosterInOfficeDaysSetupVM
                {
                    ShiftID = x.sft.ShiftID,
                    ShiftName = $"{x.sft.ShiftName} ({x.sft.StartTime} - {x.sft.EndTime})"
                }).AsNoTracking().ToListAsync();
                              
            return result;
        }
        #endregion
    }
}
