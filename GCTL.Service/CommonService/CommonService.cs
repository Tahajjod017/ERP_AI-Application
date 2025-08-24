using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        private readonly IGenericRepository<SpiralWeeklyPattern> _spiralWeeklyPatterns;
        private readonly IGenericRepository<SpiralBioWeeklyPattern> _spiralBioWeeklyPattern;
        private readonly IGenericRepository<SpiralMonthlyPattern> _spiralMonthlyPattern;
        private readonly IGenericRepository<Holidays> _holidays;
        private readonly IGenericRepository<SpiralPatternAssignList> _spiralPatternAssignList;

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
            IGenericRepository<SpiralPatternTypes> spiralPatternTypes,
            IGenericRepository<SpiralWeeklyPattern> spiralWeeklyPatterns,
            IGenericRepository<SpiralBioWeeklyPattern> spiralBioWeeklyPattern,
            IGenericRepository<SpiralMonthlyPattern> spiralMonthlyPattern,
            IGenericRepository<Holidays> holidays,
            IGenericRepository<SpiralPatternAssignList> spiralPatternAssignList)
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
            _spiralWeeklyPatterns = spiralWeeklyPatterns;
            _spiralBioWeeklyPattern = spiralBioWeeklyPattern;
            _spiralMonthlyPattern = spiralMonthlyPattern;
            _holidays = holidays;
            _spiralPatternAssignList = spiralPatternAssignList;
        }
        #endregion


        #region GetOrganizations, SearchOrganizations
        public async Task<List<CommonSelectVM>> GetOrganizations()
        {
            var result = await _organization.AllActive().AsNoTracking().Select(x => new CommonSelectVM
            {
                Id = x.OrganizationID,
                Name = x.OrganizationName ?? "-"
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
                    Name = x.OrganizationName ?? "-"
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
                Name = x.OrganizationBranchName ?? "-"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetDepartments
        public async Task<List<CommonSelectVM>> GetDepartments()
        {
            var result = await (from dep in _departments.AllActive().AsNoTracking()

                                join org in _organization.AllActive().AsNoTracking() on dep.OrganizationID equals org.OrganizationID into orgGroup
                                from org in orgGroup.DefaultIfEmpty()

                                select new CommonSelectVM
                                {
                                    Id = dep.DepartmentID,
                                    Name = dep.DepartmentName ?? "-",
                                    GroupName = org.OrganizationName ?? "-"
                                }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesGroupedByDepartment
        public async Task<List<CommonSelectVM>> GetEmpGroupedByDep()
        {
            var data = await (from emp in _employees.AllActive().AsNoTracking()

                              join empOi in _employeeOfficeInfo.AllActive() on emp.EmployeeID equals empOi.EmployeeID into empOiGroup
                              from empOi in empOiGroup.DefaultIfEmpty()

                              where emp.IsActive == true && empOi.EmploymentStatusId == 1

                              where emp.IsActive == true && empOi.EmploymentStatusId == 1

                              join org in _organization.AllActive() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                              from org in orgGroup.DefaultIfEmpty()

                              join dep in _departments.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new CommonSelectVM
                              {
                                  Id = emp.EmployeeID,
                                  Name = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})" ?? "-",
                                  GroupName = dep.DepartmentName ?? "-"
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
                Name = $"{x.ShiftName} ({x.StartTime} - {x.EndTime})" ?? "-"
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
                Name = $"{x.CompensationTypeName}" ?? "-"
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
                Name = x.SpiralPatternTypeName ?? "-"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetSpiralPatterns
        public async Task<List<CommonSelectVM>> GetSpiralPatterns()
        {
            var weekly = await _spiralWeeklyPatterns.AllActive().Include(x => x.SpiralPatternType)
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralWeeklyPatternID,
                    Name = x.SpiralWeeklyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            var biWeekly = await _spiralBioWeeklyPattern.AllActive().Include(x => x.SpiralPatternType)
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralBioWeeklyPatternID,
                    Name = x.SpiralBioWeeklyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            var monthly = await _spiralMonthlyPattern.AllActive().Include(x => x.SpiralPatternType)
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralMonthlyPatternID,
                    Name = x.SpiralMonthlyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            // Combine all into one list
            var allPatterns = weekly
                .Concat(biWeekly)
                .Concat(monthly)
                .ToList();

            return allPatterns;
        }
        #endregion


        #region GetSpiralPatternsByOrgPatternType
        public async Task<List<CommonSelectVM>> GetSpiralPatternsByOrgPatternType(int orgId, int? typeId)
        {
            var weekly = await _spiralWeeklyPatterns.AllActive().Include(x => x.SpiralPatternType)
                .Where(x => x.OrganizationID == orgId && (!typeId.HasValue || x.SpiralPatternTypeID == typeId))
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralWeeklyPatternID,
                    Name = x.SpiralWeeklyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            var biWeekly = await _spiralBioWeeklyPattern.AllActive().Include(x => x.SpiralPatternType)
                .Where(x => x.OrganizationID == orgId && (!typeId.HasValue || x.SpiralPatternTypeID == typeId))
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralBioWeeklyPatternID,
                    Name = x.SpiralBioWeeklyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            var monthly = await _spiralMonthlyPattern.AllActive().Include(x => x.SpiralPatternType)
                .Where(x => x.OrganizationID == orgId && (!typeId.HasValue || x.SpiralPatternTypeID == typeId))
                .Select(x => new CommonSelectVM
                {
                    Id = x.SpiralMonthlyPatternID,
                    Name = x.SpiralMonthlyPatternName ?? "-",
                    GroupName = x.SpiralPatternType.SpiralPatternTypeName ?? "-"
                })
                .ToListAsync();

            // Combine all into one list
            var allPatterns = weekly
                .Concat(biWeekly)
                .Concat(monthly)
                .ToList();

            return allPatterns;
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
                Name = b.OrganizationBranchName ?? "-"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetDepartmentsByOrgId
        public async Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId)
        {
            var result = await (from dep in _departments.AllActive().AsNoTracking()

                                join org in _organization.AllActive().AsNoTracking() on dep.OrganizationID equals org.OrganizationID into orgGroup
                                from org in orgGroup.DefaultIfEmpty()

                                where dep.OrganizationID == orgId

                                select new CommonSelectVM
                                {
                                    Id = dep.DepartmentID,
                                    Name = dep.DepartmentName ?? "-",
                                    GroupName = org.OrganizationName ?? "-"
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

                              where emp.IsActive == true && empOi.EmploymentStatusId == 1

                              join dep in _departments.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              where empOi.OrganizationID == orgId

                              select new CommonSelectVM
                              {
                                  Id = empOi.EmployeeID ?? 0,
                                  Name = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})" ?? "-",
                                  GroupName = dep.DepartmentName ?? "-"
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

                        where emp.IsActive == true && empOi.EmploymentStatusId == 1

                        join dep in _departments.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()

                        select new
                        {
                            empOi.OrganizationID,
                            empOi.OrganizationBranchID,
                            empOi.EmployeeID,
                            FirstName = emp.FirstName ?? "-",
                            LastName = emp.LastName ?? "-",
                            EmployeeCode = emp.EmployeeCode ?? "-",
                            DepartmentName = dep.DepartmentName ?? "-"
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
                Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})" ?? "-",
                GroupName = x.DepartmentName ?? "-"
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
                Name = $"{s.ShiftName} ({s.StartTime} - {s.EndTime})" ?? "-"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesByOrgBraDepId
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? deptIds)
        {
            var query = from emp in _employees.AllActive().AsNoTracking()

                        join empOi in _employeeOfficeInfo.AllActive().AsNoTracking() on emp.EmployeeID equals empOi.EmployeeID into empOiGroup
                        from empOi in empOiGroup.DefaultIfEmpty()

                        where emp.IsActive == true && empOi.EmploymentStatusId == 1

                        join org in _organization.AllActive().AsNoTracking() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                        from org in orgGroup.DefaultIfEmpty()

                        join bra in _organizationBranches.AllActive().AsNoTracking() on empOi.OrganizationBranchID equals bra.OrganizationBranchID into braGroup
                        from bra in braGroup.DefaultIfEmpty()
                        
                        join dep in _departments.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()
                        
                        select new
                        {
                            emp.EmployeeID,
                            FirstName = emp.FirstName ?? "-",
                            LastName = emp.LastName ?? "-",
                            EmployeeCode = emp.EmployeeCode ?? "-",
                            empOi.OrganizationID,
                            OrganizationName = org.OrganizationName ?? "-",
                            empOi.OrganizationBranchID,
                            OrganizationBranchName = bra.OrganizationBranchName ?? "-",
                            empOi.DepartmentID,
                            DepartmentName = dep.DepartmentName ?? "-"
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
                Id = x.EmployeeID,
                Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})" ?? "-",
                GroupName = x.DepartmentName ?? "-"
            }).ToListAsync();

            return result;
        }
        #endregion


        #region GetEmployeesByOrgDatesBraDepId
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgDatesBraDepId(int? orgId, List<DateTime>? dates, List<int>? branchIds, List<int>? deptIds)
        {
            try
            {
                var data = from emp in _employees.AllActive().AsNoTracking()

                           join empOi in _employeeOfficeInfo.AllActive().AsNoTracking() on emp.EmployeeID equals empOi.EmployeeID into empOiGroup
                           from empOi in empOiGroup.DefaultIfEmpty()

                           where emp.IsActive == true && empOi.EmploymentStatusId == 1

                           join org in _organization.AllActive().AsNoTracking() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                           from org in orgGroup.DefaultIfEmpty()

                           join bra in _organizationBranches.AllActive().AsNoTracking() on empOi.OrganizationBranchID equals bra.OrganizationBranchID into braGroup
                           from bra in braGroup.DefaultIfEmpty()

                           join dep in _departments.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                           from dep in depGroup.DefaultIfEmpty()

                           join spal in _spiralPatternAssignList.AllActive().AsNoTracking() on empOi.EmployeeID equals spal.EmployeeID into spalGroup
                           from spal in spalGroup.DefaultIfEmpty()

                           join hod in _holidays.AllActive().AsNoTracking() on empOi.OrganizationID equals hod.OrganizationID into hodGroup
                           from hod in hodGroup.DefaultIfEmpty()

                           select new
                           {
                               emp.EmployeeID,
                               FirstName = emp.FirstName ?? "-",
                               LastName = emp.LastName ?? "-",
                               EmployeeCode = emp.EmployeeCode ?? "-",
                               empOi.OrganizationID,
                               OrganizationName = org.OrganizationName ?? "-",
                               empOi.OrganizationBranchID,
                               OrganizationBranchName = bra.OrganizationBranchName ?? "-",
                               empOi.DepartmentID,
                               DepartmentName = dep.DepartmentName ?? "-",
                               spal.SpiralPatternAssignListID,
                               spal.StartDate,
                               spal.EndDate,
                               HolidayTitle = hod.HolidayTitle ?? "-",
                               HolidayStartDate = hod.StartDate,
                               HolidayEndDate = hod.EndDate
                           };

                if (orgId.HasValue && orgId.Value != 0)
                {
                    data = data.Where(x => x.OrganizationID == orgId.Value);
                }

                if (dates != null && dates.Any())
                {
                    data = data.Where(x => dates.Any(date => date.Date >= x.StartDate || date.Date >= x.HolidayStartDate && date.Date <= x.EndDate || date.Date <= x.HolidayEndDate));
                }
                //if (dates != null && dates.Any())
                //{
                //    data = data.Where(x => dates.Any(date => date.Date >= x.HolidayStartDate && date.Date <= x.HolidayEndDate));
                //}

                if (branchIds != null && branchIds.Any())
                {
                    data = data.Where(x => branchIds.Contains(x.OrganizationBranchID ?? 0));
                }

                if (deptIds != null && deptIds.Any())
                {
                    data = data.Where(x => deptIds.Contains(x.DepartmentID ?? 0));
                }

                var result = await data.Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})" ?? "-",
                    GroupName = x.DepartmentName ?? "-"
                }).Distinct().ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching employees", ex);
            }
        }
        #endregion


        #region GetWeekendByOrganization
        public async Task<IEnumerable<object>> GetWeekendByOrganization(int id)
        {
            try
            {
                var holidayQuery = await _holidays.AllActive()
                    .Where(x => x.OrganizationID == id && x.StatusID == 1)
                    .Include(x => x.Organization)
                    .Select(x => new
                    {
                        x.HolidayID,
                        x.Organization.OrganizationName,
                        x.StartDate,
                        x.EndDate,
                    }).ToListAsync();

                var holiday = holidayQuery.Select(x => new
                {
                    x.HolidayID,
                    OrganizationName = x.OrganizationName ?? "-",
                    TotalDays = (x.StartDate.HasValue && x.EndDate.HasValue)
                        ? string.Join(", ",
                            Enumerable.Range(0, (x.EndDate.Value - x.StartDate.Value).Days + 1)
                            .Select(offset => x.StartDate.Value.AddDays(offset).ToString("dd/MM/yy")))
                        : "-"
                });

                var weekend = await _weekendSettings.AllActive()
                    .Where(ws => ws.OrganizationID == id)
                    .Include(x => x.WeekendDays)
                    .Include(x => x.Organization)
                    .Select(x => new
                    {
                        WeekendSettingID = x.WeekendSettingID,
                        OrganizationName = x.Organization.OrganizationName ?? "-",
                        WeekdayNumbers = string.Join(", ", x.WeekendDays.Select(d => d.WeekdayNumber)) ?? "-"
                    }).ToListAsync();

                var combined = holiday.Cast<object>().Concat(weekend.Cast<object>());

                return combined;
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
