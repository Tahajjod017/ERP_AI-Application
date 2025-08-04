using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CommonService
{
    public class CommonService : ICommonService
    {
        #region Repositories
        private readonly IGenericRepository<Organization> _organization;
        private readonly IGenericRepository<OrganizationBranches> _organizationBranches;
        private readonly IGenericRepository<Departments> _departments;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
        private readonly IGenericRepository<Shifts> _shifts;
        private readonly IGenericRepository<CompensationTypes> _compensationTypes;
        private readonly IGenericRepository<WeekendSettings> _weekendSettings;
        private readonly IGenericRepository<WeekendDays> _weekendDays;
        private readonly IGenericRepository<SpiralPatternTypes> _spiralPatternTypes;

        public CommonService(
            IGenericRepository<Organization> organization,
            IGenericRepository<OrganizationBranches> organizationBranches,
            IGenericRepository<Departments> departments,
            IGenericRepository<Data.Models.Employees> employees,
            IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo,
            IGenericRepository<Shifts> shifts,
            IGenericRepository<CompensationTypes> compensationTypes,
            IGenericRepository<WeekendSettings> weekendSettings,
            IGenericRepository<WeekendDays> weekendDays,
            IGenericRepository<SpiralPatternTypes> spiralPatternTypes)
        {
            _organization = organization;
            _organizationBranches = organizationBranches;
            _departments = departments;
            _employees = employees;
            _employeeOfficeInfo = employeeOfficeInfo;
            _shifts = shifts;
            _compensationTypes = compensationTypes;
            _weekendSettings = weekendSettings;
            _weekendDays = weekendDays;
            _spiralPatternTypes = spiralPatternTypes;
        }
        #endregion


        #region GetOrganizations, SearchOrganizations
        public async Task<List<CommonSelectVM>> GetOrganizations()
        {
            var result = await _organization.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.OrganizationID,
                Name = x.OrganizationName
            }).ToListAsync();

            return result;
        }


        public async Task<PaginatedResult<CommonSelectVM>> SearchOrganizations(string search, int page = 1, int pageSize = 10)
        {
            var query = _organization.AllActive().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.OrganizationName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.OrganizationName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommonSelectVM
                {
                    Id = x.OrganizationID,
                    Name = x.OrganizationName
                })
                .ToListAsync();

            return new PaginatedResult<CommonSelectVM>
            {
                Items = items,
                HasMore = (page * pageSize) < totalCount
            };
        }
        #endregion


        #region GetBranches
        public async Task<List<CommonSelectVM>> GetBranches()
        {
            var result = await _organizationBranches.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.OrganizationBranchID,
                Name = x.OrganizationBranchName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetDepartments
        public async Task<List<CommonSelectVM>> GetDepartments()
        {
            var result = await _departments.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.DepartmentID,
                Name = x.DepartmentName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesGroupedByDepartment
        public async Task<List<CommonSelectVM>> GetEmpGroupedByDep()
        {
            var data = await (from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                              join emp in _employees.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join dep in _departments.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new CommonSelectVM
                              {
                                  Id = empOi.EmployeeID ?? 0,
                                  Name = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  GroupName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion


        #region GetShifts
        public async Task<List<CommonSelectVM>> GetShifts()
        {
            var result = await _shifts.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.ShiftID,
                Name = $"{x.ShiftName} ({x.StartTime} - {x.EndTime})"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetCompensation
        public async Task<List<CommonSelectVM>> GetCompensation()
        {
            var result = await _compensationTypes.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.CompensationTypeID,
                Name = $"{x.CompensationTypeName}"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetSpiralPatternTypes
        public async Task<List<CommonSelectVM>> GetSpiralPatternTypes()
        {
            var result = await _spiralPatternTypes.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.SpiralPatternTypeID,
                Name = x.SpiralPatternTypeName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetBranches
        public async Task<List<CommonSelectVM>> GetBranchesByOrgId(int? orgId)
        {
            var query = _organizationBranches.AllActive().AsNoTracking();

            if(orgId.HasValue && orgId.Value != 0)
                query = query.Where(b => b.OrganizationID == orgId.Value);

            var result = await query.Select(b => new CommonSelectVM
            {
                Id = b.OrganizationBranchID,
                Name = b.OrganizationBranchName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetDepartments
        public async Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId)
        {
            var query = _departments.AllActive().AsNoTracking();

            if (orgId.HasValue && orgId.Value != 0)
                query = query.Where(d => d.OrganizationID == orgId.Value);

            var result = await query.Select(d => new CommonSelectVM
            {
                Id = d.DepartmentID,
                Name = d.DepartmentName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesByOrgId
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgId(int? orgId)
        {
            var data = await (from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                              join emp in _employees.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join dep in _departments.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              where empOi.OrganizationID == orgId

                              select new CommonSelectVM
                              {
                                  Id = empOi.EmployeeID ?? 0,
                                  Name = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  GroupName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion


        #region GetEmployeesByOrgBraId
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgBraId(int? orgId, List<int>? branchIds)
        {
            var query = from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                        join emp in _employees.AllActive().AsNoTracking() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()

                        join dep in _departments.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()

                        select new
                        {
                            empOi.OrganizationID,
                            empOi.OrganizationBranchID,
                            empOi.EmployeeID,
                            emp.FirstName,
                            emp.LastName,
                            emp.EmployeeCode,
                            DepartmentName = dep.DepartmentName ?? "No Department"
                        };

            if (orgId.HasValue && orgId.Value != 0)
            {
                query = query.Where(x => x.OrganizationID == orgId.Value);
            }

            if (branchIds != null && branchIds.Any())
            {
                query = query.Where(x => branchIds.Contains(x.OrganizationBranchID ?? 0));
            }

            var data = await query.Select(x => new CommonSelectVM
            {
                Id = x.EmployeeID ?? 0,
                Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                GroupName = x.DepartmentName
            }).ToListAsync();

            return data;
        }
        #endregion


        #region GetShiftsByOrgId
        public async Task<List<CommonSelectVM>> GetShiftsByOrgId(int? orgId)
        {
            var query = _shifts.AllActive().AsNoTracking();

            if (orgId.HasValue && orgId.Value != 0)
                query = query.Where(s => s.OrganizationID == orgId.Value);

            var result = await query.Select(s => new CommonSelectVM
            {
                Id = s.ShiftID,
                Name = $"{s.ShiftName} ({s.StartTime} - {s.EndTime})"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? deptIds)
        {
            var query = from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

                        join emp in _employees.AllActive().AsNoTracking() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()
                        
                        join dep in _departments.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()
                        
                        select new
                        {
                            empOi.OrganizationID,
                            empOi.OrganizationBranchID,
                            empOi.DepartmentID,
                            empOi.EmployeeID,
                            emp.FirstName,
                            emp.LastName,
                            emp.EmployeeCode,
                            DepartmentName = dep.DepartmentName ?? "No Department"
                        };

            if (orgId.HasValue && orgId.Value != 0)
            {
                query = query.Where(x => x.OrganizationID == orgId.Value);
            }

            if (branchIds != null && branchIds.Any())
            {
                query = query.Where(x => branchIds.Contains(x.OrganizationBranchID ?? 0));
            }

            if (deptIds != null && deptIds.Any())
            {
                query = query.Where(x => deptIds.Contains(x.DepartmentID ?? 0));
            }

            var result = await query.Select(x => new CommonSelectVM
            {
                Id = x.EmployeeID ?? 0,
                Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                GroupName = x.DepartmentName
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetWeekendByOrganization
        public async Task<IEnumerable<object>> GetWeekendByOrganization(int id)
        {
            try
            {
                // Eager load related entities (Organization and WeekendDays)
                var weekendSettings = await _weekendSettings.AllActive()
                    .Include(ws => ws.Organization)
                    .Where(ws => ws.OrganizationID == id)
                    .GroupJoin(
                        _weekendDays.AllActive(),
                        ws => ws.WeekendSettingID,
                        wd => wd.WeekendSettingID,
                        (ws, days) => new
                        {
                            WeekendSettingID = ws.WeekendSettingID,
                            OrganizationName = ws.Organization.OrganizationName,
                            WeekdayNumbers = string.Join(", ", days.Select(d => d.WeekdayNumber))
                        })
                    .ToListAsync();

                return weekendSettings;
            }
            catch (Exception ex)
            {
                // You can log the error here or throw a custom exception
                throw new Exception("Error fetching weekend settings", ex);
            }
        }
        #endregion


        #region GetWeekDaysByOrganization
        public async Task<IEnumerable<object>> GetWeekDaysByOrganization(int id)
        {
            try
            {
                // Define the list of all weekdays (0 to 6)
                var allWeekdays = new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 };

                // Get the weekend settings and filter the weekend days for the given organization
                var weekendSettings = await _weekendSettings.AllActive()
                    .Include(ws => ws.Organization)
                    .Where(ws => ws.OrganizationID == id)
                    .GroupJoin(
                        _weekendDays.AllActive(),
                        ws => ws.WeekendSettingID,
                        wd => wd.WeekendSettingID,
                        (ws, days) => new
                        {
                            ws.WeekendSettingID,
                            ws.Organization.OrganizationName,
                            RemainingWeekdays = string.Join(", ", allWeekdays.Except(days.Select(d => d.WeekdayNumber ?? -1)))
                        })
                    .ToListAsync();

                return weekendSettings;
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                throw new Exception("Error fetching weekend settings", ex);
            }
        }
        #endregion
    }
}
