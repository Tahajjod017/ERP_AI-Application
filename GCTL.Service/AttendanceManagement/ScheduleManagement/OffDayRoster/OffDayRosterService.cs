using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.OffDayRoster;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class OffDayRosterService : AppService<RosterInHolyDays>, IOffDayRosterService
    {
        #region Repositories
        private readonly IGenericRepository<RosterInHolyDays> _genericRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
        private readonly IGenericRepository<CompensationDayExchanges> _compensationDayExchanges;

        public OffDayRosterService(IGenericRepository<RosterInHolyDays> genericRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo, IGenericRepository<CompensationDayExchanges> compensationDayExchanges) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employeeOfficeInfo = employeeOfficeInfo;
            _compensationDayExchanges = compensationDayExchanges;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(RosterInOffDaySetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.OrganizationID != null && model.DepartmentIDs == null && model.EmployeeIDs == null)
                {
                    foreach(var date in model.DayDate)
                    {
                        var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID);

                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(x => x.OrganizationID == employee.OrganizationID
                                && x.DepartmentID == employee.DepartmentID
                                && x.EmployeeID == employee.EmployeeID
                                && x.DayDate == date)
                                .FirstOrDefaultAsync();

                            if (model.ExchangeDate != null && model.ExchangeDate.Count > 0)
                            {
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
                                    RosterInHolyDays entity = new RosterInHolyDays();
                                    entity.OrganizationID = employee.OrganizationID;
                                    entity.DepartmentID = employee.DepartmentID;
                                    entity.EmployeeID = employee.EmployeeID;
                                    entity.ShiftID = model.ShiftID;
                                    entity.DayDate = date;
                                    entity.CompensationTypeID = model.CompensationTypeID;
                                    entity.LIP = model.LIP;
                                    entity.LMAC = model.LMAC;
                                    entity.CreatedBy = model.CreatedBy;
                                    entity.CreatedAt = DateTime.Now;
                                    await _genericRepository.AddAsync(entity);

                                    foreach (var exDate in model.ExchangeDate)
                                    {
                                        CompensationDayExchanges dayExchanges = new CompensationDayExchanges();
                                        dayExchanges.RosterInHolyDayID = entity.RosterInHolyDayID;
                                        dayExchanges.SelectDate = date;
                                        dayExchanges.ExchangeDate = exDate;
                                        dayExchanges.CreatedAt = DateTime.Now;
                                        dayExchanges.CreatedBy = model.CreatedBy;
                                        dayExchanges.LIP = model.LIP;
                                        dayExchanges.LMAC = model.LMAC;
                                        await _compensationDayExchanges.AddAsync(dayExchanges);
                                    }
                                }
                            }
                            else
                            {
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
                                    RosterInHolyDays entity = new RosterInHolyDays();
                                    entity.OrganizationID = employee.OrganizationID;
                                    entity.DepartmentID = employee.DepartmentID;
                                    entity.EmployeeID = employee.EmployeeID;
                                    entity.ShiftID = model.ShiftID;
                                    entity.DayDate = date;
                                    entity.CompensationTypeID = model.CompensationTypeID;
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
                else if (model.OrganizationID != null && model.DepartmentIDs != null && model.EmployeeIDs == null)
                {
                    foreach(var date in model.DayDate)
                    {
                        foreach (var depId in model.DepartmentIDs)
                        {
                            var employees = await _employeeOfficeInfo.FindAsync(x => x.DepartmentID == depId && x.OrganizationID == model.OrganizationID);
                            if (employees == null || !employees.Any())
                                continue;

                            foreach (var employee in employees)
                            {
                                var existingEntity = await _genericRepository.All()
                                    .Where(x => x.OrganizationID == employee.OrganizationID
                                    && x.DepartmentID == employee.DepartmentID
                                    && x.EmployeeID == employee.EmployeeID
                                    && x.DayDate == date)
                                    .FirstOrDefaultAsync();
                                if (model.ExchangeDate != null && model.ExchangeDate.Count > 0)
                                {
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
                                        RosterInHolyDays entity = new RosterInHolyDays();
                                        entity.OrganizationID = employee.OrganizationID;
                                        entity.DepartmentID = employee.DepartmentID;
                                        entity.EmployeeID = employee.EmployeeID;
                                        entity.ShiftID = model.ShiftID;
                                        entity.DayDate = date;
                                        entity.CompensationTypeID = model.CompensationTypeID;
                                        entity.LIP = model.LIP;
                                        entity.LMAC = model.LMAC;
                                        entity.CreatedBy = model.CreatedBy;
                                        entity.CreatedAt = DateTime.Now;
                                        await _genericRepository.AddAsync(entity);

                                        foreach (var exDate in model.ExchangeDate)
                                        {
                                            CompensationDayExchanges dayExchanges = new CompensationDayExchanges();
                                            dayExchanges.RosterInHolyDayID = entity.RosterInHolyDayID;
                                            dayExchanges.SelectDate = date;
                                            dayExchanges.ExchangeDate = exDate;
                                            dayExchanges.CreatedAt = DateTime.Now;
                                            dayExchanges.CreatedBy = model.CreatedBy;
                                            dayExchanges.LIP = model.LIP;
                                            dayExchanges.LMAC = model.LMAC;
                                            await _compensationDayExchanges.AddAsync(dayExchanges);
                                        }
                                    }
                                }
                                else
                                {
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
                                        RosterInHolyDays entity = new RosterInHolyDays();
                                        entity.OrganizationID = employee.OrganizationID;
                                        entity.DepartmentID = employee.DepartmentID;
                                        entity.EmployeeID = employee.EmployeeID;
                                        entity.ShiftID = model.ShiftID;
                                        entity.DayDate = date;
                                        entity.CompensationTypeID = model.CompensationTypeID;
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
                }
                else if (model.EmployeeIDs != null && model.EmployeeIDs.Any())
                {
                    foreach (var date in model.DayDate)
                    {
                        foreach (var empId in model.EmployeeIDs)
                        {
                            var employee = (await _employeeOfficeInfo.FindAsync(x => x.EmployeeID == empId)).FirstOrDefault();

                            if (employee == null || employee.DepartmentID == null) continue;

                            var existingEntity = await _genericRepository.All()
                                    .Where(x => x.OrganizationID == employee.OrganizationID
                                    && x.DepartmentID == employee.DepartmentID
                                    && x.EmployeeID == employee.EmployeeID
                                    && x.DayDate == date)
                                    .FirstOrDefaultAsync();
                            if (model.ExchangeDate != null && model.ExchangeDate.Count > 0)
                            {
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
                                    RosterInHolyDays entity = new RosterInHolyDays();
                                    entity.OrganizationID = employee.OrganizationID;
                                    entity.DepartmentID = employee.DepartmentID;
                                    entity.EmployeeID = employee.EmployeeID;
                                    entity.ShiftID = model.ShiftID;
                                    entity.DayDate = date;
                                    entity.CompensationTypeID = model.CompensationTypeID;
                                    entity.LIP = model.LIP;
                                    entity.LMAC = model.LMAC;
                                    entity.CreatedBy = model.CreatedBy;
                                    entity.CreatedAt = DateTime.Now;
                                    await _genericRepository.AddAsync(entity);

                                    foreach (var exDate in model.ExchangeDate)
                                    {
                                        CompensationDayExchanges dayExchanges = new CompensationDayExchanges();
                                        dayExchanges.RosterInHolyDayID = entity.RosterInHolyDayID;
                                        dayExchanges.SelectDate = date;
                                        dayExchanges.ExchangeDate = exDate;
                                        dayExchanges.CreatedAt = DateTime.Now;
                                        dayExchanges.CreatedBy = model.CreatedBy;
                                        dayExchanges.LIP = model.LIP;
                                        dayExchanges.LMAC = model.LMAC;
                                        await _compensationDayExchanges.AddAsync(dayExchanges);
                                    }
                                }
                            }
                            else
                            {
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
                                    RosterInHolyDays entity = new RosterInHolyDays();
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


        #region UpdateEmpShiftAsync
        public async Task<bool> UpdateEmpShiftAsync(RosterInOffDayEditVM model)
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
                        RosterInHolyDays entity = new RosterInHolyDays();
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


        #region GetAllAsync
        public async Task<(List<RosterInOffDayListVM> Data, List<string> UniqueDates, SeparatePaginationInfo Pagination)> GetAllAsync(
            int pageNumber = 1, 
            int pageSize = 5, 
            string searchTerm = "", 
            string sortColumn = "RosterInHolyDayID", 
            string sortOrder = "desc", 
            int daysToShow = 7,
            DateTime? startDate = null)
        {
            // 1. Get raw flat records first
            var rawData = await _genericRepository.AllActive()
                .Where(x => x.DayDate.HasValue)
                .Select(x => new
                {
                    x.EmployeeID,
                    EmployeeName = $"{x.Employee.FirstName} {x.Employee.LastName} ({x.Employee.EmployeeCode})",
                    x.DepartmentID,
                    DepartmentName = x.Department.DepartmentName,
                    x.OrganizationID,
                    OrganizationName = x.Organization.OrganizationName,
                    x.DayDate,
                    x.ShiftID,
                    ShiftName = x.Shift.ShiftName,
                    StartTime = x.Shift.StartTime,
                    EndTime = x.Shift.EndTime,
                    x.RosterInHolyDayID
                })
                .AsNoTracking()
                .ToListAsync();

            // 2. Extract unique dates
            var startFrom = startDate ?? DateTime.Today;

            var uniqueDates = rawData
                .Where(x => x.DayDate.Value >= startDate)
                .Select(x => x.DayDate.Value.Date.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(d => d)
                .Take(daysToShow)
                .ToList();

            // 3. Group data in memory (small set after projection)
            var grouped = rawData
                .GroupBy(x => x.EmployeeID)
                .Select(g => new RosterInOffDayListVM
                {
                    EmployeeID = g.Key ?? 0,
                    RosterInHolyDayID = g.First().RosterInHolyDayID,
                    EmployeeName = g.First().EmployeeName ?? "",
                    DepartmentID = g.First().DepartmentID ?? 0,
                    DepartmentName = g.First().DepartmentName ?? "",
                    OrganizationID = g.First().OrganizationID ?? 0,
                    OrganizationName = g.First().OrganizationName ?? "",
                    ShiftsPerDay = g
                        .Where(x => x.DayDate.HasValue)
                        .ToDictionary(
                            k => k.DayDate.Value.ToString("yyyy-MM-dd"),
                            v => new ShiftVM
                            {
                                ShiftID = v.ShiftID,
                                ShiftName = v.ShiftName ?? "N/A",
                                TimeRange = $"{v.StartTime?.ToString(@"hh\:mm")} - {v.EndTime?.ToString(@"hh\:mm")}",
                                RosterInHolyDayID = v.RosterInHolyDayID
                            })
                })
                .ToList();

            // 4. Apply search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
                grouped = grouped
                    .Where(x =>
                        x.EmployeeName.ToLower().Contains(lowerSearch) ||
                        x.DepartmentName.ToLower().Contains(lowerSearch) ||
                        x.OrganizationName.ToLower().Contains(lowerSearch))
                    .ToList();
            }

            // 5. Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                grouped = sortColumn.ToLower() switch
                {
                    "employeename" => (sortOrder == "asc"
                        ? grouped.OrderBy(x => x.EmployeeName)
                        : grouped.OrderByDescending(x => x.EmployeeName)).ToList(),

                    "departmentname" => (sortOrder == "asc"
                        ? grouped.OrderBy(x => x.DepartmentName)
                        : grouped.OrderByDescending(x => x.DepartmentName)).ToList(),

                    "organizationname" => (sortOrder == "asc"
                        ? grouped.OrderBy(x => x.OrganizationName)
                        : grouped.OrderByDescending(x => x.OrganizationName)).ToList(),

                    _ => grouped.OrderByDescending(x => x.RosterInHolyDayID).ToList() // default fallback
                };
            }

            // 4. Paginate grouped results
            var totalCount = grouped.Count;
            var pagedResult = pageSize == 0
                ? grouped
                : grouped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // 5. Build pagination info
            var pagination = new SeparatePaginationInfo
            {
                StartItem = (pageNumber - 1) * pageSize + 1,
                EndItem = Math.Min(pageNumber * pageSize, totalCount),
                TotalItems = totalCount,
                CurrentPage = pageNumber,
                TotalPages = pageSize == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumbers = pageSize == 0
                    ? new List<int> { 1 }
                    : Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
            };

            return (pagedResult, uniqueDates, pagination);
        }
        #endregion
    }
}
