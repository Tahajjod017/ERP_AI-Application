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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OffDayRoster
{
    public class OffDayRosterService : AppService<RosterInHolyDays>, IOffDayRosterService
    {
        #region Repositories
        private readonly IGenericRepository<RosterInHolyDays> _genericRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;

        public OffDayRosterService(IGenericRepository<RosterInHolyDays> genericRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employeeOfficeInfo = employeeOfficeInfo;
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


        #region GetAllAsync
        public async Task<SeparatePaginationResult<RosterInOffDayListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOffDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null)
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

                return new RosterInOffDayListVM
                {
                    RosterInOffDayID = first.RosterInHolyDayID,
                    OrganizationID = first.OrganizationID,
                    OrganizationName = org?.OrganizationName ?? "-",
                    DepartmentID = first.DepartmentID,
                    DepartmentName = first.Department?.DepartmentName ?? "-",
                    EmployeeID = first.EmployeeID ?? 0,
                    EmployeeName = $"{first.Employee?.FirstName} {first.Employee?.LastName} ({first.Employee?.EmployeeCode})",
                    ShiftID = first.ShiftID ?? 0,
                    ShiftName = first.Shift?.ShiftName ?? "-",
                    TimeRange = first.Shift != null ? $"{first.Shift.StartTime:hh\\:mm} - {first.Shift.EndTime:hh\\:mm}" : "-",
                    //DayDate = string.Join(",", DayDate),
                    //WeekdayNumber = string.Join(",", weekendNumbers),
                    //HolidayDates = string.Join(",", holidayEntries.Select(h => h.Date.ToString("yyyy-MM-dd"))),
                    //HolidayTitle = string.Join(",", holidayEntries.Select(h => h.Title))
                };
            }).ToList();

            var totalCount = transformed.Count;
            var pagedData = transformed
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SeparatePaginationResult<RosterInOffDayListVM>
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


        public async Task<(List<RosterInOffDayListVM> rosterList, List<string> uniqueDates)> GetAll()
        {
            var rosterData = _genericRepository.AllActive()
                .Include(x => x.Organization)
                .Include(x => x.Employee)
                .Include(x => x.Department)
                .Include(x => x.Shift)
                .Where(x => x.DayDate.HasValue)
                .ToList();

            var uniqueDates = rosterData
                .Select(x => x.DayDate.Value.Date.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var rosterVM = rosterData
                .GroupBy(x => x.EmployeeID)
                .Select(g => new RosterInOffDayListVM
                {
                    EmployeeID = g.Key ?? 0,
                    EmployeeName = g.First().Employee?.FirstName ?? "",
                    DepartmentName = g.First().Department?.DepartmentName ?? "",
                    OrganizationName = g.First().Organization?.OrganizationName ?? "",
                    ShiftsPerDay = g
                        .Where(x => x.DayDate.HasValue)
                        .ToDictionary(
                            k => k.DayDate.Value,
                            v => new ShiftVM
                            {
                                ShiftName = v.Shift?.ShiftName ?? "N/A",
                                TimeRange = $"{v.Shift?.StartTime?.ToString(@"hh\:mm")} - {v.Shift?.EndTime?.ToString(@"hh\:mm")}"
                            }
                        )
                })
                .ToList();

            return (rosterVM, uniqueDates);
        }
    }
}
