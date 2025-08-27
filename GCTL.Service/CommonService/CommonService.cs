using Azure;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using QuestPDF.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
        private readonly IGenericRepository<SpiralWeeklyPatternDetails> _spiralWeeklyPatternDetails;
        private readonly IGenericRepository<SpiralBioWeeklyPatternDetails> _spiralBioWeeklyPatternDetails;
        private readonly IGenericRepository<SpiralMonthlyPatternDetails> _spiralMonthlyPatternDetails;

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
            IGenericRepository<SpiralPatternAssignList> spiralPatternAssignList,
            IGenericRepository<SpiralWeeklyPatternDetails> spiralWeeklyPatternDetails,
            IGenericRepository<SpiralBioWeeklyPatternDetails> spiralBioWeeklyPatternDetails,
            IGenericRepository<SpiralMonthlyPatternDetails> spiralMonthlyPatternDetails)
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
            _spiralWeeklyPatternDetails = spiralWeeklyPatternDetails;
            _spiralBioWeeklyPatternDetails = spiralBioWeeklyPatternDetails;
            _spiralMonthlyPatternDetails = spiralMonthlyPatternDetails;
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


        public async Task<List<CommonSelectVM>> SearchEmployees(string search, int pageSize = 50)
        {
            var employees = await (from emp in _employees.AllActive().AsNoTracking()

                                   join empOi in _employeeOfficeInfo.AllActive() on emp.EmployeeID equals empOi.EmployeeID into empOiGroup
                                   from empOi in empOiGroup.DefaultIfEmpty()

                                   where emp.IsActive == true && empOi.EmploymentStatusId == 1 &&
                                         (string.IsNullOrEmpty(search) || (emp.FirstName + " " + emp.LastName + " " + emp.EmployeeCode).Contains(search))

                                   join dep in _departments.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                                   from dep in depGroup.DefaultIfEmpty()

                                   orderby emp.FirstName

                                   select new CommonSelectVM
                                   {
                                       Id = emp.EmployeeID,
                                       Name = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})" ?? "-",
                                       GroupName = dep.DepartmentName ?? "-"
                                   })
                           .Take(pageSize) // limit per request
                           .ToListAsync();

            return employees;
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
            var baseQuery = _employeeOfficeInfo.AllActive().AsNoTracking().Where(eoi => eoi.EmploymentStatusId == 1);

            if (orgId.HasValue && orgId.Value != 0)
                baseQuery = baseQuery.Where(eoi => eoi.OrganizationID == orgId.Value);

            if (branchIds != null && branchIds.Any())
                baseQuery = baseQuery.Where(eoi => eoi.OrganizationBranchID.HasValue && branchIds.Contains(eoi.OrganizationBranchID.Value));

            if (deptIds != null && deptIds.Any())
                baseQuery = baseQuery.Where(eoi => eoi.DepartmentID.HasValue &&
                                                   deptIds.Contains(eoi.DepartmentID.Value));

            var result = await (from eoi in baseQuery

                                join emp in _employees.AllActive().AsNoTracking().Where(emp => emp.IsActive == true) on eoi.EmployeeID equals emp.EmployeeID
                                
                                join dep in _departments.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                                from dep in depGroup.DefaultIfEmpty()
                                
                                select new CommonSelectVM
                                {
                                    Id = emp.EmployeeID,
                                    Name = $"{emp.FirstName ?? "-"} {emp.LastName ?? "-"} ({emp.EmployeeCode ?? "-"})",
                                    GroupName = dep.DepartmentName ?? "-"
                                })
                               .ToListAsync();

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

                           join wes in _weekendSettings.AllActive().AsNoTracking() on empOi.OrganizationID equals wes.OrganizationID into wesGroup
                           from wes in wesGroup.DefaultIfEmpty()

                           join wed in _weekendDays.AllActive().AsNoTracking() on wes.WeekendSettingID equals wed.WeekendSettingID into wedGroup
                           from wed in wedGroup.DefaultIfEmpty()

                           join hod in _holidays.AllActive().AsNoTracking() on empOi.OrganizationID equals hod.OrganizationID into hodGroup
                           from hod in hodGroup.DefaultIfEmpty()

                           join spal in _spiralPatternAssignList.AllActive().AsNoTracking() on empOi.EmployeeID equals spal.EmployeeID into spalGroup
                           from spal in spalGroup.DefaultIfEmpty()

                           join swp in _spiralWeeklyPatterns.AllActive().AsNoTracking() on spal.SpiralWeeklyPatternID equals swp.SpiralWeeklyPatternID into swpGroup
                           from swp in swpGroup.DefaultIfEmpty()

                           join swpd in _spiralWeeklyPatternDetails.AllActive().AsNoTracking() on swp.SpiralWeeklyPatternID equals swpd.SpiralWeeklyPatternID into swpdGroup
                           from swpd in swpdGroup.DefaultIfEmpty()

                           join sbp in _spiralBioWeeklyPattern.AllActive().AsNoTracking() on spal.SpiralBioWeeklyPatternID equals sbp.SpiralBioWeeklyPatternID into sbpGroup
                           from sbp in sbpGroup.DefaultIfEmpty()

                           join sbpd in _spiralBioWeeklyPatternDetails.AllActive().AsNoTracking() on sbp.SpiralBioWeeklyPatternID equals sbpd.SpiralBioWeeklyPatternID into sbpdGroup
                           from sbpd in sbpdGroup.DefaultIfEmpty()

                           join smp in _spiralMonthlyPattern.AllActive().AsNoTracking() on spal.SpiralMonthlyPatternID equals smp.SpiralMonthlyPatternID into smpGroup
                           from smp in smpGroup.DefaultIfEmpty()

                           join smpd in _spiralMonthlyPatternDetails.AllActive().AsNoTracking() on smp.SpiralMonthlyPatternID equals smpd.SpiralMonthlyPatternID into smpdGroup
                           from smpd in smpdGroup.DefaultIfEmpty()

                           join swpdSft in _shifts.AllActive().AsNoTracking() on swpd.ShiftID equals swpdSft.ShiftID into swpdSftGroup
                           from swpdSft in swpdSftGroup.DefaultIfEmpty()

                           join sbpdSft in _shifts.AllActive().AsNoTracking() on sbpd.ShiftID equals sbpdSft.ShiftID into sbpdSftGroup
                           from sbpdSft in sbpdSftGroup.DefaultIfEmpty()

                           join smpdSft in _shifts.AllActive().AsNoTracking() on smpd.ShiftID equals smpdSft.ShiftID into smpdSftGroup
                           from smpdSft in smpdSftGroup.DefaultIfEmpty()

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
                               weekendDay = wed.WeekdayNumber,
                               HolidayTitle = hod.HolidayTitle ?? "-",
                               HolidayStartDate = hod.StartDate,
                               HolidayEndDate = hod.EndDate,
                               //WeeklyPatternDay = swpd != null ? swpd.DayOfWeek : (int?)null,

                               // Spiral Pattern Data
                               SpiralOrganizationID = spal.OrganizationID,
                               SpiralStartDate = spal.StartDate,
                               SpiralEndDate = spal.EndDate,
                               SpiralWeeklyPatternID = spal.SpiralWeeklyPatternID,
                               SpiralBioWeeklyPatternID = spal.SpiralBioWeeklyPatternID,
                               SpiralMonthlyPatternID = spal.SpiralMonthlyPatternID,
                               SpiralWeeklyPatternDay = swpd != null ? swpd.DayOfWeek : (int?)null,
                               SpiralWeeklyShiftName = swpdSft.ShiftName,
                               SpiralBioWeeklyPatternDay = sbpd != null ? sbpd.DayOfMonth : (int?)null,
                               SpiralBioWeeklyShiftName = sbpdSft.ShiftName,
                               SpiralMonthlyPatternDay = smpd != null ? smpd.DayOfMonth : (int?)null,
                               SpiralMonthlyShiftName = smpdSft.ShiftName,
                           };

                // Apply organization filter
                if (orgId.HasValue && orgId.Value != 0)
                {
                    data = data.Where(x => x.OrganizationID == orgId.Value);
                }

                // Apply branch filter
                if (branchIds != null && branchIds.Any())
                {
                    data = data.Where(x => branchIds.Contains(x.OrganizationBranchID ?? 0));
                }

                // Apply department filter
                if (deptIds != null && deptIds.Any())
                {
                    data = data.Where(x => deptIds.Contains(x.DepartmentID ?? 0));
                }

                // Materialize the query
                var resultList = await data.ToListAsync();

                // === Date filters ===
                if (dates != null && dates.Any())
                {
                    resultList = resultList.Where(x =>
                        dates.Any(date =>
                        {
                            int weekday = (int)date.DayOfWeek;
                            int dayOfMonth = date.Day;

                            // Weekend Match
                            bool isWeekend = x.weekendDay.HasValue && weekday == x.weekendDay.Value;

                            // Holiday Match
                            bool isHoliday = x.HolidayStartDate.HasValue && x.HolidayEndDate.HasValue &&
                                             date.Date >= x.HolidayStartDate.Value.Date &&
                                             date.Date <= x.HolidayEndDate.Value.Date;

                            // Spiral Pattern Match for "Weekend" shift
                            bool isSpiralMatch =
                                x.SpiralOrganizationID == 1 &&
                                x.SpiralStartDate.HasValue && x.SpiralEndDate.HasValue &&
                                date.Date >= x.SpiralStartDate.Value.Date &&
                                date.Date <= x.SpiralEndDate.Value.Date &&
                                (
                                    (x.SpiralWeeklyPatternID != null && x.SpiralWeeklyPatternDay == weekday && x.SpiralWeeklyShiftName == "Weekend")
                                    ||
                                    (x.SpiralBioWeeklyPatternID != null && x.SpiralBioWeeklyPatternDay == dayOfMonth && x.SpiralBioWeeklyShiftName == "Weekend")
                                    ||
                                    (x.SpiralMonthlyPatternID != null && x.SpiralMonthlyPatternDay == dayOfMonth && x.SpiralMonthlyShiftName == "Weekend")
                                );

                            return isWeekend || isHoliday || isSpiralMatch;
                        })
                    ).ToList();
                }

                // Final projection to view model
                var result = resultList
                    .GroupBy(x => x.EmployeeID)
                    .Select(x => x.First())
                    .Select(x => new CommonSelectVM
                    {
                        Id = x.EmployeeID,
                        Name = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                        GroupName = x.DepartmentName
                    })
                    .ToList();

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
                        x.HolidayTitle,
                        x.Organization.OrganizationName,
                        x.StartDate,
                        x.EndDate,
                    }).ToListAsync();

                var holiday = holidayQuery.Select(x => new
                {
                    x.HolidayID,
                    x.HolidayTitle,
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
