using Dapper;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OfficeDayRoster;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly IGenericRepository<WeekendSettings> _weekendSettings;
        private readonly IGenericRepository<WeekendDays> _weekendDays;
        private readonly IGenericRepository<Holidays> _holidays;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<OrganizationBranches> _branchRepository;
        private readonly IDbConnection _dbCon;

        public OfficeDayRosterService(IGenericRepository<RosterInOfficeDays> genericRepository,
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Data.Models.Employees> employeesRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo,
            IGenericRepository<Shifts> shiftsRepository,
            IConfiguration configuration,
            IGenericRepository<OrganizationBranches> branchRepository,
            IDbConnection dbCon,
            IGenericRepository<WeekendSettings> weekendSettings,
            IGenericRepository<WeekendDays> weekendDays,
            IGenericRepository<Holidays> holidays) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            _employeesRepository = employeesRepository;
            _employeeOfficeInfo = employeeOfficeInfo;
            _shiftsRepository = shiftsRepository;
            _configuration = configuration;
            _branchRepository = branchRepository;
            _dbCon = new SqlConnection(configuration.GetConnectionString("connection"));
            _weekendSettings = weekendSettings;
            _weekendDays = weekendDays;
            _holidays = holidays;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(RosterInOfficeDaysSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.OrganizationID != null && model.DepartmentIDs == null && model.EmployeeIDs == null)
                {
                    var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID);

                    for (var date = model.StartDate.Value; date <= model.EndDate.Value; date = date.AddDays(1))
                    {
                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(x => x.OrganizationID == employee.OrganizationID
                                && x.DepartmentID == employee.DepartmentID
                                && x.EmployeeID == employee.EmployeeID
                                && x.DayDate == date)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                                existingEntity.ShiftID = model.ShiftID;
                                existingEntity.LIP = model.LIP;
                                existingEntity.LMAC = model.LMAC;
                                existingEntity.CreatedBy = model.CreatedBy;
                                existingEntity.CreatedAt = DateTime.Now;

                                await _genericRepository.UpdateAsync(existingEntity);
                            }
                            else
                            {
                                RosterInOfficeDays entity = new RosterInOfficeDays();
                                entity.OrganizationID = employee.OrganizationID;
                                entity.DepartmentID = employee.DepartmentID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.ShiftID = model.ShiftID;
                                entity.DayDate = date;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                await _genericRepository.AddAsync(entity);
                            }
                        }
                    }
                }
                else if (model.OrganizationID != null && model.DepartmentIDs != null && model.EmployeeIDs == null)
                {
                    foreach (var depId in model.DepartmentIDs)
                    {
                        var employees = await _employeeOfficeInfo.FindAsync(x => x.DepartmentID == depId && x.OrganizationID == model.OrganizationID);
                        if (employees == null || !employees.Any())
                            continue;

                        for(var date  = model.StartDate.Value; date <= model.EndDate.Value; date = date.AddDays(1))
                        {
                            foreach (var employee in employees)
                            {
                                var existingEntity = await _genericRepository.All()
                                    .Where(x => x.OrganizationID == employee.OrganizationID 
                                    && x.DepartmentID == employee.DepartmentID 
                                    && x.EmployeeID == employee.EmployeeID
                                    && x.DayDate == date)
                                    .FirstOrDefaultAsync();
                                if (existingEntity != null)
                                {
                                    existingEntity.ShiftID = model.ShiftID;
                                    existingEntity.LIP = model.LIP;
                                    existingEntity.LMAC = model.LMAC;
                                    existingEntity.CreatedBy = model.CreatedBy;
                                    existingEntity.CreatedAt = DateTime.Now;

                                    await _genericRepository.UpdateAsync(existingEntity);
                                }
                                else
                                {
                                    RosterInOfficeDays entity = new RosterInOfficeDays();
                                    entity.OrganizationID = employee.OrganizationID;
                                    entity.DepartmentID = employee.DepartmentID;
                                    entity.EmployeeID = employee.EmployeeID;
                                    entity.ShiftID = model.ShiftID;
                                    entity.DayDate = date;
                                    entity.LIP = model.LIP;
                                    entity.LMAC = model.LMAC;
                                    entity.CreatedBy = model.CreatedBy;
                                    entity.CreatedAt = DateTime.Now;
                                    await _genericRepository.AddAsync(entity);
                                }
                            }
                        }
                    }
                }
                else if (model.EmployeeIDs != null && model.EmployeeIDs.Any())
                {
                    foreach (var empId in model.EmployeeIDs)
                    {
                        var employee = (await _employeeOfficeInfo.FindAsync(x => x.EmployeeID == empId)).FirstOrDefault();

                        if (employee == null || employee.DepartmentID == null) continue;

                        for(var date = model.StartDate.Value; date <= model.EndDate.Value; date = date.AddDays(1))
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(x => x.OrganizationID == employee.OrganizationID 
                                && x.DepartmentID == employee.DepartmentID 
                                && x.EmployeeID == employee.EmployeeID
                                && x.DayDate == date)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                                existingEntity.ShiftID = model.ShiftID;
                                existingEntity.LIP = model.LIP;
                                existingEntity.LMAC = model.LMAC;
                                existingEntity.CreatedBy = model.CreatedBy;
                                existingEntity.CreatedAt = DateTime.Now;

                                await _genericRepository.UpdateAsync(existingEntity);
                            }
                            else
                            {
                                RosterInOfficeDays entity = new RosterInOfficeDays();
                                entity.OrganizationID = employee.OrganizationID;
                                entity.DepartmentID = employee.DepartmentID;
                                entity.EmployeeID = empId;
                                entity.ShiftID = model.ShiftID;
                                entity.DayDate = date;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;

                                await _genericRepository.AddAsync(entity);
                            }
                        }
                    }
                }

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
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


        #region AddEmpShiftAsync
        public async Task<bool> AddEmpShiftAsync(RosterInOfficeDayModalAddVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var employees = await _employeeOfficeInfo
                    .FindAsync(x => x.OrganizationID == model.OrganizationIdAdd
                    && x.DepartmentID == model.DepartmentIdAdd
                    && x.EmployeeID == model.EmployeeIdAdd);

                foreach (var employee in employees)
                {
                    var existingEntity = await _genericRepository.All()
                        .Where(x => x.OrganizationID == employee.OrganizationID
                        && x.DepartmentID == employee.DepartmentID
                        && x.EmployeeID == employee.EmployeeID
                        && x.DayDate == model.DayDateAdd)
                        .FirstOrDefaultAsync();
                    if (existingEntity != null)
                    {
                        existingEntity.ShiftID = model.ShiftIdAdd;
                        existingEntity.LIP = model.LIP;
                        existingEntity.LMAC = model.LMAC;
                        existingEntity.CreatedBy = model.CreatedBy;
                        existingEntity.CreatedAt = DateTime.Now;

                        await _genericRepository.UpdateAsync(existingEntity);
                    }
                    else
                    {
                        RosterInOfficeDays entity = new RosterInOfficeDays();
                        entity.OrganizationID = employee.OrganizationID;
                        entity.DepartmentID = employee.DepartmentID;
                        entity.EmployeeID = employee.EmployeeID;
                        entity.ShiftID = model.ShiftIdAdd;
                        entity.DayDate = model.DayDateAdd;
                        entity.LIP = model.LIP;
                        entity.LMAC = model.LMAC;
                        entity.CreatedBy = model.CreatedBy;
                        entity.CreatedAt = DateTime.Now;
                        await _genericRepository.AddAsync(entity);
                    }
                }

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


        #region UpdateEmpShiftAsync
        public async Task<bool> UpdateEmpShiftAsync(RosterInOfficeDayEditVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var employees = await _employeeOfficeInfo
                    .FindAsync(x => x.OrganizationID == model.OrganizationIdEdit
                    && x.DepartmentID == model.DepartmentIdEdit
                    && x.EmployeeID == model.EmployeeIdEdit);

                foreach (var employee in employees)
                {
                    var existingEntity = await _genericRepository.All()
                        .Where(x => x.OrganizationID == employee.OrganizationID
                        && x.DepartmentID == employee.DepartmentID
                        && x.EmployeeID == employee.EmployeeID
                        && x.DayDate == model.DayDateEdit)
                        .FirstOrDefaultAsync();
                    if (existingEntity != null)
                    {
                        existingEntity.ShiftID = model.ShiftIdEdit;
                        existingEntity.LIP = model.LIP;
                        existingEntity.LMAC = model.LMAC;
                        existingEntity.CreatedBy = model.CreatedBy;
                        existingEntity.CreatedAt = DateTime.Now;

                        await _genericRepository.UpdateAsync(existingEntity);
                    }
                    else
                    {
                        RosterInOfficeDays entity = new RosterInOfficeDays();
                        entity.OrganizationID = employee.OrganizationID;
                        entity.DepartmentID = employee.DepartmentID;
                        entity.EmployeeID = employee.EmployeeID;
                        entity.ShiftID = model.ShiftIdEdit;
                        entity.DayDate = model.DayDateEdit;
                        entity.LIP = model.LIP;
                        entity.LMAC = model.LMAC;
                        entity.CreatedBy = model.CreatedBy;
                        entity.CreatedAt = DateTime.Now;
                        await _genericRepository.AddAsync(entity);
                    }
                }

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


        #region GetPagedEmployeesAsync
        public async Task<PaginationResult2<RosterInOfficeDaysListVM>> GetPagedEmployeesAsync(int pageNumber, int pageSize, string searchTerm, int daysToShow = 7, DateTime? startDate = null)
        {
            try
            {
                var result = new PaginationResult2<RosterInOfficeDaysListVM>();

                var parameters = new DynamicParameters();
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);
                parameters.Add("@SearchTerm", searchTerm);

                using var multi = await _dbCon.QueryMultipleAsync("Prc_GetEmployeesPaged", parameters, commandType: CommandType.StoredProcedure);
                result.Data = (await multi.ReadAsync<RosterInOfficeDaysListVM>()).ToList();
                result.TotalCount = await multi.ReadFirstAsync<int>();

                return result;
            }
            catch (Exception ex)
            {
                // Return an empty PaginationResult2 object in case of an exception
                return new PaginationResult2<RosterInOfficeDaysListVM>
                {
                    Data = new List<RosterInOfficeDaysListVM>(),
                    TotalCount = 0
                };
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<SeparatePaginationResult<RosterInOfficeDaysListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;
            var endDate = startDate.Value.AddDays(daysToShow);

            var query = _genericRepository.AllActive().AsNoTracking()
                .Include(x => x.Shift)
                .Include(x => x.Organization).ThenInclude(x => x.WeekendSettings).ThenInclude(x => x.WeekendDays)
                .Include(x => x.Organization).ThenInclude(x => x.Holidays)
                .Include(x => x.Department)
                .Include(x => x.Employee).ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                .Where(x => x.DeletedAt == null && x.DayDate >= startDate || x.DayDate < endDate);

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();

                query = query.Where(x =>
                    EF.Functions.Like(x.Shift.ShiftName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Organization.OrganizationName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Employee.FirstName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Employee.LastName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Employee.EmployeeCode, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Department.DepartmentName, $"%{searchTerm}%")
                );
            }

            // Group by EmployeeID
            var grouped = await query
                .GroupBy(x => x.EmployeeID)
                .ToListAsync();

            var transformed = grouped.Select(g =>
            {
                var first = g.First();
                var assignedDates = g.Select(x =>
                    $"{x.DayDate:yyyy-MM-dd}|{x.Shift?.ShiftName ?? "-"}|{(x.Shift != null ? $"{x.Shift.StartTime:hh\\:mm} - {x.Shift.EndTime:hh\\:mm}" : "-")}")
                    .ToList();

                var org = first.Organization;
                var weekendNumbers = org?.WeekendSettings?
                    .SelectMany(ws => ws.WeekendDays)
                    .Select(wd => (int)wd.WeekdayNumber)
                    .Where(n => n >= 0 && n <= 6)
                    .Distinct()
                    .ToList() ?? new List<int>();

                var holidayEntries = org?.Holidays?
                    .Where(h => h.StartDate <= endDate && h.EndDate >= startDate)
                    .SelectMany(h =>
                    {
                        var entries = new List<(DateTime Date, string Title)>();
                        for (var date = h.StartDate.Value; date <= h.EndDate.Value; date = date.AddDays(1))
                        {
                            entries.Add((date, h.HolidayTitle));
                        }
                        return entries;
                    })
                    .Where(h => h.Date >= startDate && h.Date < endDate)
                    .ToList();

                return new RosterInOfficeDaysListVM
                {
                    RosterInOfficeDayID = first.RosterInOfficeDayID,
                    OrganizationID = first.OrganizationID,
                    OrganizationName = org?.OrganizationName ?? "-",
                    DepartmentID = first.DepartmentID,
                    DepartmentName = first.Department?.DepartmentName ?? "-",
                    EmployeeID = first.EmployeeID ?? 0,
                    EmployeeName = $"{first.Employee?.FirstName} {first.Employee?.LastName} ({first.Employee?.EmployeeCode})",
                    ShiftID = first.ShiftID ?? 0,
                    ShiftName = first.Shift?.ShiftName ?? "-",
                    TimeRange = first.Shift != null ? $"{first.Shift.StartTime:hh\\:mm} - {first.Shift.EndTime:hh\\:mm}" : "-",
                    AssignedDates = string.Join(",", assignedDates),
                    WeekdayNumber = string.Join(",", weekendNumbers),
                    HolidayDates = string.Join(",", holidayEntries.Select(h => h.Date.ToString("yyyy-MM-dd"))),
                    HolidayTitle = string.Join(",", holidayEntries.Select(h => h.Title))
                };
            }).ToList();

            var totalCount = transformed.Count;
            var pagedData = transformed
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SeparatePaginationResult<RosterInOfficeDaysListVM>
            {
                Data = pagedData,
                TotalCount = totalCount,
                SeparatePaginationInfo = new SeparatePaginationInfo
                {
                    StartItem = (pageNumber - 1) * pageSize + 1,
                    EndItem = Math.Min(pageNumber * pageSize, totalCount),
                    TotalItems = totalCount,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    PageNumbers = Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
                }
            };
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
    }
}
