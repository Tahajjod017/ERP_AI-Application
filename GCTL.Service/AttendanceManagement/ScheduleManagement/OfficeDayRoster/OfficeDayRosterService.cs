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
                var existingEntity = await _genericRepository.All()
                        .Where(x => x.OrganizationID == model.OrganizationIdAdd
                        && x.DepartmentID == model.DepartmentIdAdd
                        && x.EmployeeID == model.EmployeeIdAdd
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
                    entity.OrganizationID = model.OrganizationIdAdd;
                    entity.DepartmentID = model.DepartmentIdAdd;
                    entity.EmployeeID = model.EmployeeIdAdd;
                    entity.ShiftID = model.ShiftIdAdd;
                    entity.DayDate = model.DayDateAdd;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy;
                    entity.CreatedAt = DateTime.Now;
                    await _genericRepository.AddAsync(entity);
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
        public async Task<SeparatePaginationResult<RosterInOfficeDaysListVM>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            int daysToShow = 7,
            DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;
            var endDate = startDate.Value.AddDays(daysToShow);

            // Base query with projection to lightweight DTO
            var baseQuery = _genericRepository.AllActive()
                .AsNoTracking()
                .Where(x => x.DeletedAt == null && x.DayDate >= startDate && x.DayDate < endDate)
                .Select(x => new
                {
                    x.EmployeeID,
                    EmployeeCode = x.Employee.EmployeeCode,
                    EmployeeFirstName = x.Employee.FirstName,
                    EmployeeLastName = x.Employee.LastName,
                    x.OrganizationID,
                    OrganizationName = x.Organization.OrganizationName,
                    WeekendSettings = x.Organization.WeekendSettings.SelectMany(ws => ws.WeekendDays).Select(wd => (int)wd.WeekdayNumber).Distinct(),
                    Holidays = x.Organization.Holidays.Where(h => h.StartDate <= endDate && h.EndDate >= startDate).Select(h => new { h.StartDate, h.EndDate, h.HolidayTitle }),
                    Leaves = x.Employee.LeaveApplicationsEmployee.Where(l => l.IsFinalApproved == true)
                        .Select(l => new
                        {
                            l.LeaveApplicationID,
                            l.FromDate,
                            l.ToDate,
                            l.IsFullDay,
                            l.PartialFromTime,
                            l.PartialToTime,
                            LeaveTypeName = l.LeaveType != null ? l.LeaveType.LeaveTypeName : null
                        }),
                    x.DepartmentID,
                    DepartmentName = x.Department.DepartmentName,
                    x.ShiftID,
                    ShiftName = x.Shift.ShiftName,
                    ShiftStartTime = x.Shift.StartTime,
                    ShiftEndTime = x.Shift.EndTime,
                    x.DayDate,
                    x.RosterInOfficeDayID
                });

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    EF.Functions.Like(x.ShiftName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.OrganizationName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.EmployeeFirstName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.EmployeeLastName, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.EmployeeCode, $"%{searchTerm}%") ||
                    EF.Functions.Like(x.DepartmentName, $"%{searchTerm}%")
                );
            }

            // Get total count for pagination
            var totalCount = await baseQuery.Select(x => x.EmployeeID).Distinct().CountAsync();

            // Fetch paged EmployeeIDs
            var pagedEmployeeIds = await baseQuery.Select(x => x.EmployeeID).Distinct().OrderBy(id => id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            if (!pagedEmployeeIds.Any())
                return new SeparatePaginationResult<RosterInOfficeDaysListVM>
                {
                    Data = new List<RosterInOfficeDaysListVM>(),
                    TotalCount = 0,
                    SeparatePaginationInfo = new SeparatePaginationInfo
                    {
                        StartItem = 0,
                        EndItem = 0,
                        TotalItems = 0,
                        CurrentPage = pageNumber,
                        TotalPages = 0,
                        PageNumbers = new List<int>()
                    }
                };

            // Fetch roster data for selected employees
            var rosterData = await baseQuery.Where(x => pagedEmployeeIds.Contains(x.EmployeeID)).ToListAsync();

            // Group in memory
            var grouped = rosterData.GroupBy(x => x.EmployeeID);

            var result = grouped.Select(g =>
            {
                var first = g.First();

                // Assigned dates
                var assignedDates = g.Select(x =>
                    $"{x.DayDate:yyyy-MM-dd}|{x.ShiftName ?? "-"}|{(x.ShiftName != null ? $"{x.ShiftStartTime:hh\\:mm} - {x.ShiftEndTime:hh\\:mm}" : "-")}"
                );

                // Weekend numbers
                var weekendNumbers = g.SelectMany(x => x.WeekendSettings).Distinct();

                // Expand holidays
                var holidayEntries = g.SelectMany(x => x.Holidays)
                    .SelectMany(h =>
                    {
                        var list = new List<(DateTime Date, string Title)>();
                        for (var d = h.StartDate.Value; d <= h.EndDate.Value; d = d.AddDays(1))
                            if (d >= startDate && d < endDate)
                                list.Add((d, h.HolidayTitle));
                        return list;
                    }).ToList();

                // Expand leaves
                var leaveEntries = g.SelectMany(x => x.Leaves)
                    .SelectMany(l =>
                    {
                        var list = new List<(DateTime Date, string LeaveInfo)>();
                        var from = l.FromDate.ToDateTime(TimeOnly.MinValue);
                        var to = l.ToDate.ToDateTime(TimeOnly.MinValue);

                        for (var d = from; d <= to; d = d.AddDays(1))
                        {
                            if (d >= startDate && d < endDate)
                            {
                                string leaveInfo;

                                if (l.IsFullDay)
                                {
                                    leaveInfo = l.LeaveTypeName ?? "-";
                                }
                                else
                                {
                                    // Include partial time for partial-day leave
                                    var fromTime = l.PartialFromTime?.ToString("hh\\:mm") ?? "-";
                                    var toTime = l.PartialToTime?.ToString("hh\\:mm") ?? "-";
                                    leaveInfo = $"{l.LeaveTypeName ?? "-"} ({fromTime} - {toTime})";
                                }

                                list.Add((d, leaveInfo));
                            }
                        }
                        return list;
                    }).OrderBy(l => l.Date)
                    .ToList();

                // Make sure leaveEntries are ordered by Date
                //var orderedLeaveEntries = leaveEntries.OrderBy(l => l.Date).Distinct().ToList();

                // Map to LeaveDates and LeaveTypeName in same order
                var orderedLeaveDates = leaveEntries.Select(l => l.Date.ToString("yyyy-MM-dd"));
                var orderedLeaveTypes = leaveEntries.Select(l => l.LeaveInfo);
                
                return new RosterInOfficeDaysListVM
                {
                    RosterInOfficeDayID = first.RosterInOfficeDayID,
                    OrganizationID = first.OrganizationID,
                    OrganizationName = first.OrganizationName ?? "-",
                    DepartmentID = first.DepartmentID,
                    DepartmentName = first.DepartmentName ?? "-",
                    EmployeeID = first.EmployeeID ?? 0,
                    EmployeeName = $"{first.EmployeeFirstName} {first.EmployeeLastName} ({first.EmployeeCode})",
                    ShiftID = first.ShiftID ?? 0,
                    AssignedDates = string.Join(",", assignedDates),
                    WeekdayNumber = string.Join(",", weekendNumbers),
                    HolidayDates = string.Join(",", holidayEntries.Select(h => h.Date.ToString("yyyy-MM-dd")).Distinct()),
                    HolidayTitle = string.Join(",", holidayEntries.Select(h => h.Title).Distinct()),
                    //LeaveDates = string.Join(",", orderedLeaveEntries.Select(l => l.Date.ToString("yyyy-MM-dd"))),
                    //LeaveTypeName = string.Join(",", orderedLeaveEntries.Select(l => l.LeaveTypeName ?? "-"))
                    LeaveDates = string.Join(",", orderedLeaveDates),
                    LeaveTypeName = string.Join(",", orderedLeaveTypes)
                };
            }).ToList();

            return new SeparatePaginationResult<RosterInOfficeDaysListVM>
            {
                Data = result,
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
