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
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.OfficeDayRoster
{
    //public class OfficeDayRosterService : AppService<RosterInOfficeDays>, IOfficeDayRosterService
    //{
    //    #region Repositories
    //    private readonly IGenericRepository<RosterInOfficeDays> _genericRepository;
    //    private readonly IGenericRepository<Organization> _organizationRepository;
    //    private readonly IGenericRepository<Departments> _departmentRepository;
    //    private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;
    //    private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfo;
    //    private readonly IGenericRepository<Shifts> _shiftsRepository;
    //    //private readonly IGenericRepository<RosterInOfficeDaysOverride> _rosterInOfficeDayOverride;
    //    private readonly IConfiguration _configuration;

    //    public OfficeDayRosterService(IGenericRepository<RosterInOfficeDays> genericRepository,
    //        IGenericRepository<Organization> organizationRepository,
    //        IGenericRepository<Departments> departmentRepository,
    //        IGenericRepository<Data.Models.Employees> employeesRepository,
    //        IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfo,
    //        IGenericRepository<Shifts> shiftsRepository,
    //        IGenericRepository<RosterInOfficeDaysOverride> rosterInOfficeDayOverride,
    //        IConfiguration configuration) : base(genericRepository)
    //    {
    //        _genericRepository = genericRepository;
    //        _organizationRepository = organizationRepository;
    //        _departmentRepository = departmentRepository;
    //        _employeesRepository = employeesRepository;
    //        _employeeOfficeInfo = employeeOfficeInfo;
    //        _shiftsRepository = shiftsRepository;
    //        _rosterInOfficeDayOverride = rosterInOfficeDayOverride;
    //        _configuration = configuration;
    //    }
    //    #endregion


    //    #region AddAsync
    //    public async Task<bool> AddAsync(RosterInOfficeDaysSetupVM model)
    //    {
    //        await _genericRepository.BeginTransactionAsync();
    //        try
    //        {
    //            //var startDate = DateTime.ParseExact(model.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

    //            if (model.OrganizationID != null && model.DepartmentIDs == null && model.EmployeeIDs == null)
    //            {
    //                var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID);
    //                //if (employees == null || !employees.Any())
    //                //    continue;
    //                foreach (var employee in employees)
    //                {
    //                    //if (model.ExcludedEmployeeIDs != null && model.ExcludedEmployeeIDs.Contains(employee.EmployeeID ?? 0))
    //                    //    continue;

    //                    var existingEntity = await _genericRepository.All()
    //                        .Where(x => x.OrganizationID == employee.OrganizationID && x.DepartmentID == employee.DepartmentID && x.EmployeeID == employee.EmployeeID && x.StartDate == model.StartDate && x.EndDate == model.EndDate).FirstOrDefaultAsync();
    //                    if (existingEntity != null)
    //                    {
    //                        existingEntity.ShiftID = model.ShiftID;
    //                        existingEntity.LIP = model.LIP;
    //                        existingEntity.LMAC = model.LMAC;
    //                        existingEntity.CreatedBy = model.CreatedBy;
    //                        existingEntity.CreatedAt = DateTime.Now;

    //                        await _genericRepository.UpdateAsync(existingEntity);
    //                    }
    //                    else
    //                    {
    //                        RosterInOfficeDays entity = new RosterInOfficeDays();
    //                        entity.OrganizationID = employee.OrganizationID;
    //                        entity.DepartmentID = employee.DepartmentID;
    //                        entity.EmployeeID = employee.EmployeeID;
    //                        entity.ShiftID = model.ShiftID;
    //                        entity.StartDate = model.StartDate;
    //                        entity.EndDate = model.EndDate;
    //                        entity.LIP = model.LIP;
    //                        entity.LMAC = model.LMAC;
    //                        entity.CreatedBy = model.CreatedBy;
    //                        entity.CreatedAt = DateTime.Now;
    //                        await _genericRepository.AddAsync(entity);
    //                    }
    //                }
    //            }
    //            else if (model.OrganizationID != null && model.DepartmentIDs != null && model.EmployeeIDs == null)
    //            {
    //                foreach (var depId in model.DepartmentIDs)
    //                {
    //                    var employees = await _employeeOfficeInfo.FindAsync(x => x.DepartmentID == depId && x.OrganizationID == model.OrganizationID);
    //                    if (employees == null || !employees.Any())
    //                        continue;
    //                    foreach (var employee in employees)
    //                    {
    //                        var existingEntity = await _genericRepository.All().Where(x => x.OrganizationID == employee.OrganizationID && x.DepartmentID == employee.DepartmentID && x.EmployeeID == employee.EmployeeID).FirstOrDefaultAsync();
    //                        if (existingEntity != null)
    //                        {
    //                            existingEntity.ShiftID = model.ShiftID;
    //                            existingEntity.LIP = model.LIP;
    //                            existingEntity.LMAC = model.LMAC;
    //                            existingEntity.CreatedBy = model.CreatedBy;
    //                            existingEntity.CreatedAt = DateTime.Now;

    //                            await _genericRepository.UpdateAsync(existingEntity);
    //                        }
    //                        else
    //                        {
    //                            RosterInOfficeDays entity = new RosterInOfficeDays();
    //                            entity.ShiftID = model.ShiftID;
    //                            entity.OrganizationID = employee.OrganizationID;
    //                            entity.DepartmentID = employee.DepartmentID;
    //                            entity.EmployeeID = employee.EmployeeID;
    //                            entity.LIP = model.LIP;
    //                            entity.LMAC = model.LMAC;
    //                            entity.CreatedBy = model.CreatedBy;
    //                            entity.CreatedAt = DateTime.Now;
    //                            await _genericRepository.AddAsync(entity);
    //                        }
    //                    }
    //                }
    //            }
    //            else if (model.EmployeeIDs != null && model.EmployeeIDs.Any())
    //            {
    //                foreach (var empId in model.EmployeeIDs)
    //                {
    //                    var employee = (await _employeeOfficeInfo.FindAsync(x => x.EmployeeID == empId)).FirstOrDefault();

    //                    if (employee == null || employee.DepartmentID == null) continue;

    //                    var existingEntity = await _genericRepository.All().Where(x => x.OrganizationID == employee.OrganizationID && x.DepartmentID == employee.DepartmentID && x.EmployeeID == employee.EmployeeID).FirstOrDefaultAsync();
    //                    if (existingEntity != null)
    //                    {
    //                        existingEntity.ShiftID = model.ShiftID;
    //                        existingEntity.LIP = model.LIP;
    //                        existingEntity.LMAC = model.LMAC;
    //                        existingEntity.CreatedBy = model.CreatedBy;
    //                        existingEntity.CreatedAt = DateTime.Now;

    //                        await _genericRepository.UpdateAsync(existingEntity);
    //                    }
    //                    else
    //                    {
    //                        RosterInOfficeDays entity = new RosterInOfficeDays();
    //                        entity.OrganizationID = employee.OrganizationID;
    //                        entity.DepartmentID = employee.DepartmentID;
    //                        entity.EmployeeID = empId;
    //                        entity.ShiftID = model.ShiftID;

    //                        entity.LIP = model.LIP;
    //                        entity.LMAC = model.LMAC;
    //                        entity.CreatedBy = model.CreatedBy;
    //                        entity.CreatedAt = DateTime.Now;

    //                        await _genericRepository.AddAsync(entity);
    //                    }
    //                }
    //            }

    //            await _genericRepository.CommitTransactionAsync();
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            await _genericRepository.RollbackTransactionAsync();
    //            return false;
    //        }
    //    }
    //    #endregion


    //    #region UpdateAsync
    //    public async Task<bool> UpdateAsync(RosterInOfficeDaysSetupVM model)
    //    {
    //        await _genericRepository.BeginTransactionAsync();
    //        try
    //        {
    //            var entity = await _genericRepository.GetByIdAsync(model.RosterInOfficeDayID);
    //            if (entity == null)
    //            {
    //                return false;
    //            }

    //            entity.OrganizationID = model.OrganizationID;
    //            entity.DepartmentID = model.DepartmentIDs.FirstOrDefault();
    //            entity.EmployeeID = model.EmployeeIDs.FirstOrDefault();
    //            entity.ShiftID = model.ShiftID;

    //            await _genericRepository.UpdateAsync(entity);
    //            await _genericRepository.CommitTransactionAsync();
    //            return true;
    //        }
    //        catch
    //        {
    //            await _genericRepository.RollbackTransactionAsync();
    //            return false;
    //        }
    //    }
    //    #endregion


    //    #region UpdateAsync
    //    public async Task<bool> UpdateEmpShiftAsync(RosterInOfficeDaysOverrideSetupVM model)
    //    {
    //        await _genericRepository.BeginTransactionAsync();
    //        try
    //        {
    //            var data = await _genericRepository.FindAsync(x => x.RosterInOfficeDayID == model.RosterInOfficeDayID);
    //            if (data == null || data.Count == 0)
    //            {
    //                return false;
    //            }

    //            foreach (var item in data)
    //            {
    //                var existingOverride = await _rosterInOfficeDayOverride.FirstOrDefaultAsync(x =>
    //                    x.RosterInOfficeDayID == model.RosterInOfficeDayID &&
    //                    x.OverrideDate == model.OverrideDate);

    //                if (existingOverride != null)
    //                {
    //                    existingOverride.ShiftID = model.ShiftID;
    //                    existingOverride.UpdatedAt = DateTime.Now;
    //                    existingOverride.UpdatedBy = model.CreatedBy ?? null;

    //                    await _rosterInOfficeDayOverride.UpdateAsync(existingOverride);
    //                }
    //                else
    //                {
    //                    var newOverride = new RosterInOfficeDaysOverride
    //                    {
    //                        RosterInOfficeDayID = item.RosterInOfficeDayID,
    //                        OverrideDate = model.OverrideDate,
    //                        ShiftID = model.ShiftID,
    //                        CreatedAt = DateTime.Now,
    //                        CreatedBy = model.CreatedBy ?? null,
    //                        LIP = item.LIP,
    //                        LMAC = item.LMAC
    //                    };

    //                    await _rosterInOfficeDayOverride.AddAsync(newOverride);
    //                }
    //            }

    //            await _genericRepository.CommitTransactionAsync();
    //            return true;
    //        }
    //        catch
    //        {
    //            await _genericRepository.RollbackTransactionAsync();
    //            return false;
    //        }
    //    }


    //    //public async Task<bool> UpdateEmpShiftAsync(RosterInOfficeDaysOverrideSetupVM model)
    //    //{
    //    //    await _genericRepository.BeginTransactionAsync();
    //    //    try
    //    //    {
    //    //        var data = await _genericRepository.FindAsync(x => x.RosterInOfficeDayID == model.RosterInOfficeDayID);
    //    //        if (data == null || data.Count == 0)
    //    //        {
    //    //            return false;
    //    //        }

    //    //        // 🔍 Remove existing override for the same day (if any)
    //    //        var existingOverrides = await _rosterInOfficeDayOverride.FindAsync(x =>
    //    //            x.RosterInOfficeDayID == model.RosterInOfficeDayID &&
    //    //            x.OverrideDate == model.OverrideDate);

    //    //        if (existingOverrides != null && existingOverrides.Count > 0)
    //    //        {
    //    //            await _rosterInOfficeDayOverride.DeleteRangeAsync(existingOverrides);
    //    //        }

    //    //        // ➕ Add new override
    //    //        var overrideList = new List<RosterInOfficeDaysOverride>();

    //    //        foreach (var item in data)
    //    //        {
    //    //            var overrideEntry = new RosterInOfficeDaysOverride
    //    //            {
    //    //                RosterInOfficeDayID = item.RosterInOfficeDayID,
    //    //                OverrideDate = model.OverrideDate,
    //    //                ShiftID = model.ShiftID,
    //    //                CreatedAt = DateTime.Now,
    //    //                CreatedBy = model.CreatedBy ?? null,
    //    //                LIP = item.LIP,
    //    //                LMAC = item.LMAC
    //    //            };

    //    //            overrideList.Add(overrideEntry);
    //    //        }

    //    //        await _rosterInOfficeDayOverride.AddRangeAsync(overrideList);

    //    //        await _genericRepository.CommitTransactionAsync();
    //    //        return true;
    //    //    }
    //    //    catch
    //    //    {
    //    //        await _genericRepository.RollbackTransactionAsync();
    //    //        return false;
    //    //    }
    //    //}
    //    #endregion


    //    #region GetAllFromSPAsync
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetAllFromSPAsync(int pageNumber, int pageSize, string searchTerm, string sortColumn, string sortOrder, int daysToShow)
    //    {
    //        using (var connection = new SqlConnection(_configuration.GetConnectionString("connection")))
    //        {
    //            var result = await connection.QueryAsync<RosterInOfficeDaysSetupVM>(
    //                "GetPagedRosterInOfficeDays",
    //                new
    //                {
    //                    PageNumber = pageNumber,
    //                    PageSize = pageSize,
    //                    SearchTerm = searchTerm,
    //                    SortColumn = sortColumn,
    //                    SortOrder = sortOrder,
    //                    DaysToShow = daysToShow
    //                },
    //                commandType: CommandType.StoredProcedure
    //            );

    //            return result.ToList();
    //        }
    //    }
    //    #endregion


    //    public class PaginationInfo2
    //    {
    //        public int StartItem { get; set; }
    //        public int EndItem { get; set; }
    //        public int TotalItems { get; set; }
    //        public List<int> PageNumbers { get; set; }
    //        public int TotalPages { get; set; }
    //        public int CurrentPage { get; set; }
    //    }


    //    public class RosterEmployeeGroupedVM
    //    {
    //        public int EmployeeID { get; set; }
    //        public string EmployeeName { get; set; }
    //        public string DepartmentName { get; set; }
    //        public string OrganizationName { get; set; }

    //        public List<RosterInOfficeDaysSetupVM> ShiftCells { get; set; } = new();
    //    }



    //    public async Task<(List<RosterEmployeeGroupedVM> Data, PaginationInfo2 Pagination)> GetAllGroupedAsync(
    //    int pageNumber = 1,
    //    int pageSize = 5,
    //    string searchTerm = "",
    //    string sortColumn = "RosterInOfficeDayID",
    //    string sortOrder = "desc",
    //    int daysToShow = 7,
    //    DateTime? startDate = null)
    //    {
    //        var start = startDate ?? DateTime.Today;
    //        var end = start.AddDays(daysToShow - 1);

    //        // Fetch from DB
    //        var query = _genericRepository.AllActive().AsNoTracking()
    //            .Include(x => x.Shift)
    //            .Include(x => x.Organization)
    //            .Include(x => x.Department)
    //            .Include(x => x.Employee)
    //            .Include(x => x.RosterInOfficeDaysOverride)
    //                .ThenInclude(o => o.Shift)
    //            .Where(x => x.StartDate <= end && x.EndDate >= start);

    //        // Optional filter
    //        if (!string.IsNullOrWhiteSpace(searchTerm))
    //        {
    //            query = query.Where(x =>
    //                EF.Functions.Like(x.Shift.ShiftName, $"%{searchTerm}%") ||
    //                EF.Functions.Like(x.Organization.OrganizationName, $"%{searchTerm}%") ||
    //                EF.Functions.Like(x.Employee.FirstName, $"%{searchTerm}%") ||
    //                EF.Functions.Like(x.Employee.LastName, $"%{searchTerm}%") ||
    //                EF.Functions.Like(x.Employee.EmployeeCode, $"%{searchTerm}%") ||
    //                EF.Functions.Like(x.Department.DepartmentName, $"%{searchTerm}%"));
    //        }

    //        var records = await query.ToListAsync();

    //        // Flatten shift records by day
    //        var flattened = new List<RosterInOfficeDaysSetupVM>();

    //        foreach (var item in records)
    //        {
    //            var current = item.StartDate;
    //            while (current <= item.EndDate)
    //            {
    //                if (current.Value.Date == DateTime.Today)
    //                {
    //                    Console.WriteLine("✅ Including current date: " + current.Value.ToShortDateString());
    //                }

    //                if (current >= start && current <= end)
    //                {
    //                    flattened.Add(new RosterInOfficeDaysSetupVM
    //                    {
    //                        RosterInOfficeDayID = item.RosterInOfficeDayID,
    //                        OrganizationID = item.OrganizationID,
    //                        OrganizationName = item.Organization?.OrganizationName ?? "-",
    //                        DepartmentID = item.DepartmentID,
    //                        DepartmentName = item.Department?.DepartmentName ?? "-",
    //                        EmployeeID = item.EmployeeID,
    //                        EmployeeName = $"{item.Employee?.FirstName} {item.Employee?.LastName} ({item.Employee?.EmployeeCode})",
    //                        ShiftID = item.ShiftID,
    //                        ShiftName = item.Shift?.ShiftName ?? "-",
    //                        StartDate = current,
    //                        EndDate = current,
    //                        TimeRange = $"{item.Shift?.StartTime:hh\\:mm} - {item.Shift?.EndTime:hh\\:mm}",
    //                        RosterInOfficeDaysOverrideSetupVMs = item.RosterInOfficeDaysOverride?
    //                            .Where(o => o.OverrideDate?.Date == current.Value.Date)
    //                            .Select(o => new RosterInOfficeDaysOverrideSetupVM
    //                            {
    //                                RosterInOfficeDaysOverrideID = o.RosterInOfficeDaysOverrideID,
    //                                RosterInOfficeDayID = o.RosterInOfficeDayID ?? 0,
    //                                OverrideDate = o.OverrideDate,
    //                                ShiftID = o.ShiftID ?? 0,
    //                                ShiftName = o.Shift?.ShiftName ?? "",
    //                                TimeRange = $"{o.Shift?.StartTime:hh\\:mm} - {o.Shift?.EndTime:hh\\:mm}"
    //                            }).ToList()
    //                    });
    //                }

    //                current = current?.AddDays(1);
    //            }
    //        }

    //        // Group by Employee
    //        var grouped = flattened
    //            .GroupBy(x => x.EmployeeID)
    //            .Select(g => new RosterEmployeeGroupedVM
    //            {
    //                EmployeeID = g.Key ?? 0,
    //                EmployeeName = g.First().EmployeeName,
    //                DepartmentName = g.First().DepartmentName,
    //                OrganizationName = g.First().OrganizationName,
    //                ShiftCells = g.ToList()
    //            })
    //            .ToList();

    //        var totalItems = grouped.Count;
    //        var pagedGroups = grouped
    //            .Skip((pageNumber - 1) * pageSize)
    //            .Take(pageSize)
    //            .ToList();

    //        var pagination = new PaginationInfo2
    //        {
    //            TotalItems = totalItems,
    //            CurrentPage = pageNumber,
    //            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
    //            StartItem = (pageNumber - 1) * pageSize + 1,
    //            EndItem = Math.Min(pageNumber * pageSize, totalItems),
    //            PageNumbers = Enumerable.Range(1, (int)Math.Ceiling((double)totalItems / pageSize)).ToList()
    //        };

    //        return (pagedGroups, pagination);
    //    }




    //    #region GetAllAsync
    //    public async Task<PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.PaginationResult<RosterInOfficeDaysSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "RosterInOfficeDayID", string sortOrder = "desc", int daysToShow = 7, DateTime? startDate = null)
    //    {
    //        var start = startDate ?? DateTime.Today;
    //        var end = start.AddDays(daysToShow - 1);

    //        var query = _genericRepository.AllActive().AsNoTracking()
    //            .Include(x => x.Shift)
    //            .Include(x => x.Organization)
    //            .Include(x => x.Department)
    //            .Include(x => x.Employee)
    //            .Include(x => x.RosterInOfficeDaysOverride)
    //                .ThenInclude(o => o.Shift)
    //            .Where(x => x.StartDate <= end && x.EndDate >= start); // Filter within selected range

    //        // Apply sorting
    //        if (!string.IsNullOrEmpty(sortColumn))
    //        {
    //            query = sortColumn switch
    //            {
    //                "RosterInOfficeDayID" => sortOrder == "desc" ? query.OrderByDescending(x => x.RosterInOfficeDayID) : query.OrderBy(x => x.RosterInOfficeDayID),
    //                "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Shift.ShiftName) : query.OrderBy(x => x.Shift.ShiftName),
    //                "OrganizationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Organization.OrganizationName) : query.OrderBy(x => x.Organization.OrganizationName),
    //                "DepartmentName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Department.DepartmentName) : query.OrderBy(x => x.Department.DepartmentName),
    //                "EmployeeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Employee.FirstName) : query.OrderBy(x => x.Employee.FirstName),
    //                _ => query.OrderBy(x => x.ShiftID)
    //            };
    //        }

    //        // Paginate and project to ViewModel
    //        var result = await PaginationService<RosterInOfficeDays, RosterInOfficeDaysSetupVM>.GetPaginatedData(
    //            query,
    //            pageNumber,
    //            pageSize,
    //            searchTerm,
    //            sortColumn,
    //            sortOrder,
    //            term => x => EF.Functions.Like(x.Shift.ShiftName, $"%{term}%")
    //                      || EF.Functions.Like(x.Organization.OrganizationName, $"%{term}%")
    //                      || EF.Functions.Like(x.Employee.FirstName, $"%{term}%")
    //                      || EF.Functions.Like(x.Employee.LastName, $"%{term}%")
    //                      || EF.Functions.Like(x.Employee.EmployeeCode, $"%{term}%")
    //                      || EF.Functions.Like(x.Department.DepartmentName, $"%{term}%"),
    //            x => new RosterInOfficeDaysSetupVM
    //            {
    //                RosterInOfficeDayID = x.RosterInOfficeDayID,
    //                OrganizationID = x.OrganizationID,
    //                OrganizationName = x.Organization.OrganizationName ?? "-",
    //                DepartmentName = x.Department.DepartmentName ?? "-",
    //                EmployeeName = $"{x.Employee.FirstName} {x.Employee.LastName} ({x.Employee.EmployeeCode})",
    //                ShiftName = x.Shift.ShiftName ?? "-",
    //                ShiftID = x.ShiftID ?? 0,
    //                EmployeeID = x.EmployeeID ?? 0,
    //                StartDate = x.StartDate,
    //                EndDate = x.EndDate,
    //                TimeRange = $"{x.Shift.StartTime:hh\\:mm} - {x.Shift.EndTime:hh\\:mm}",
    //                RosterInOfficeDaysOverrideSetupVMs = x.RosterInOfficeDaysOverride
    //                    .Where(o => o.ShiftID != null && o.RosterInOfficeDayID != null && o.OverrideDate != null)
    //                    .Select(o => new RosterInOfficeDaysOverrideSetupVM
    //                    {
    //                        RosterInOfficeDaysOverrideID = o.RosterInOfficeDaysOverrideID,
    //                        RosterInOfficeDayID = o.RosterInOfficeDayID ?? 0,
    //                        OverrideDate = o.OverrideDate ?? null,
    //                        ShiftID = o.ShiftID ?? 0,
    //                        ShiftName = o.Shift?.ShiftName ?? "",
    //                        TimeRange = $"{o.Shift?.StartTime:hh\\:mm} - {o.Shift?.EndTime:hh\\:mm}"
    //                    }).Distinct().ToList()
    //            });

    //        return result;
    //    }
    //    #endregion


    //    #region GetByIdAsync
    //    public async Task<RosterInOfficeDaysSetupVM> GetByIdAsync(int id)
    //    {
    //        var entity = await _genericRepository.GetByIdAsync(id);
    //        var defaultShift = entity as RosterInOfficeDays;

    //        if (defaultShift == null)
    //            return null;

    //        return new RosterInOfficeDaysSetupVM
    //        {
    //            RosterInOfficeDayID = defaultShift.RosterInOfficeDayID,
    //            ShiftID = defaultShift.ShiftID,
    //            OrganizationID = defaultShift.OrganizationID,
    //            DepartmentID = defaultShift.DepartmentID,
    //            EmployeeID = defaultShift.EmployeeID
    //        };
    //    }
    //    #endregion


    //    #region GetCompanies
    //    public Task<List<CommonSelectVM>> GetCompanies()
    //    {
    //        var data = _organizationRepository.AllActive()
    //            .Select(x => new CommonSelectVM
    //            {
    //                Id = x.OrganizationID,
    //                Name = x.OrganizationName
    //            }).ToListAsync();
    //        return data;
    //    }
    //    #endregion


    //    #region GetDepartments
    //    public async Task<List<CommonSelectVM>> GetDepartments()
    //    {
    //        var data = await _departmentRepository.AllActive()
    //            .Select(x => new CommonSelectVM
    //            {
    //                Id = x.DepartmentID,
    //                Name = x.DepartmentName
    //            }).ToListAsync();
    //        return data;
    //    }
    //    #endregion


    //    #region GetGroupedEmployees
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetGroupedEmployees()
    //    {
    //        var data = await (from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()

    //                          join emp in _employeesRepository.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
    //                          from emp in empGroup.DefaultIfEmpty()

    //                          join org in _organizationRepository.AllActive() on empOi.OrganizationID equals org.OrganizationID into orgGroup
    //                          from org in orgGroup.DefaultIfEmpty()

    //                          join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
    //                          from dep in depGroup.DefaultIfEmpty()

    //                          select new RosterInOfficeDaysSetupVM
    //                          {
    //                              EmployeeID = empOi.EmployeeID ?? 0,
    //                              EmployeeName = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
    //                              DepartmentName = dep.DepartmentName
    //                          }).ToListAsync();
    //        return data;
    //    }
    //    #endregion


    //    #region GetFilteredEmployees
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByDepartment(int? orgId, List<int>? departmentIds)
    //    {
    //        var query = from empOi in _employeeOfficeInfo.AllActive().AsNoTracking()
                        
    //                    join emp in _employeesRepository.AllActive().AsNoTracking() on empOi.EmployeeID equals emp.EmployeeID into empGroup
    //                    from emp in empGroup.DefaultIfEmpty()

    //                    join dep in _departmentRepository.AllActive().AsNoTracking() on empOi.DepartmentID equals dep.DepartmentID into depGroup
    //                    from dep in depGroup.DefaultIfEmpty()

    //                    select new
    //                    {
    //                        empOi.EmployeeID,
    //                        emp.FirstName,
    //                        emp.LastName,
    //                        emp.EmployeeCode,
    //                        dep.DepartmentName,
    //                        empOi.OrganizationID,
    //                        empOi.DepartmentID
    //                    };

    //        if (departmentIds?.Any() == true)
    //            query = query.Where(x => x.OrganizationID == orgId && departmentIds.Contains(x.DepartmentID ?? 0));

    //        return await query
    //            .Select(x => new RosterInOfficeDaysSetupVM
    //            {
    //                EmployeeID = x.EmployeeID ?? 0,
    //                EmployeeName = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
    //                DepartmentName = x.DepartmentName
    //            }).AsNoTracking().ToListAsync();
    //    }
    //    #endregion


    //    #region GetShift
    //    public async Task<List<CommonSelectVM>> GetShift()
    //    {
    //        var data = await _shiftsRepository.AllActive()
    //            .Select(x => new CommonSelectVM
    //            {
    //                Id = x.ShiftID,
    //                Name = $"{x.ShiftName} ({x.StartTime} - {x.EndTime})"
    //            }).ToListAsync();
    //        return data;
    //    }
    //    #endregion


    //    #region SoftDeleteAsync
    //    //public async Task<RosterInOfficeDaysSetupVM> SoftDeleteAsync(RosterDelVM model)
    //    //{
    //    //    await _genericRepository.BeginTransactionAsync();
    //    //    try
    //    //    {
    //    //        var data = await _genericRepository.FindAsync(x => x.RosterInOfficeDayID == model.Id);
    //    //        if(data == null || data.Count == 0)
    //    //        {
    //    //            return new RosterInOfficeDaysSetupVM
    //    //            {
    //    //                Message = "No data found to delete."
    //    //            };
    //    //        }

    //    //        var overrideList = new List<RosterInOfficeDaysOverride>();

    //    //        foreach (var item in data)
    //    //        {
    //    //            //item.ShiftID = null;
    //    //            //item.DeletedAt = DateTime.Now;
    //    //            //item.DeletedBy = model.DeletedBy ?? null;
    //    //            var overrideEntry = new RosterInOfficeDaysOverride
    //    //            {
    //    //                RosterInOfficeDayID = item.RosterInOfficeDayID,
    //    //                OverrideDate = model.OverrideDate, 
    //    //                ShiftID = null,
    //    //                DeletedAt = DateTime.Now,
    //    //                DeletedBy = model.DeletedBy ?? null,
    //    //                LIP = item.LIP,
    //    //                LMAC = item.LMAC
    //    //            };

    //    //            overrideList.Add(overrideEntry);
    //    //        }

    //    //        //await _genericRepository.UpdateRangeAsync(data);
    //    //        await _rosterInOfficeDayOverride.AddRangeAsync(overrideList);

    //    //        await _genericRepository.CommitTransactionAsync();

    //    //        return new RosterInOfficeDaysSetupVM
    //    //        {
    //    //            Message = $"{data.Count} shift deleted successfully."
    //    //        };
    //    //    }
    //    //    catch(Exception ex)
    //    //    {
    //    //        await _genericRepository.RollbackTransactionAsync();
    //    //        throw new Exception("Error occured during the deletion of data.", ex);
    //    //    }
    //    //}
    //    #endregion


    //    #region GetDepartmentByOrganization
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetDepartmentByOrganization(int? id)
    //    {
    //        var query = from eoi in _employeeOfficeInfo.AllActive().AsNoTracking()

    //                    join dep in _departmentRepository.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
    //                    from dep in depGroup.DefaultIfEmpty()

    //                    select new { eoi, dep };

    //        if (id.HasValue && id.Value != 0)
    //        {
    //            query = query.Where(x => x.eoi.OrganizationID == id.Value);
    //        }

    //        var result = await query
    //            .Select(x => new RosterInOfficeDaysSetupVM
    //            {
    //                DepartmentID = x.eoi.DepartmentID ?? 0,
    //                DepartmentName = x.dep.DepartmentName
    //            }).Distinct().AsNoTracking().ToListAsync();

    //        return result;
    //    }

    //    #endregion


    //    #region GetEmployeeByOrganization
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetEmployeeByOrganization(int? id)
    //    {
    //        var query = from eoi in _employeeOfficeInfo.AllActive().AsNoTracking()

    //                    join emp in _employeesRepository.AllActive().AsNoTracking() on eoi.EmployeeID equals emp.EmployeeID into empGroup
    //                    from emp in empGroup.DefaultIfEmpty()

    //                    join dep in _departmentRepository.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
    //                    from dep in depGroup.DefaultIfEmpty()

    //                    select new { eoi, emp, dep };

    //        if(id.HasValue && id.Value != 0)
    //        {
    //            query = query.Where(x => x.eoi.OrganizationID == id.Value);
    //        }

    //        var result = await query
    //            .Select(x => new RosterInOfficeDaysSetupVM
    //            {
    //                EmployeeID = x.eoi.EmployeeID,
    //                EmployeeName = $"{x.emp.FirstName} {x.emp.LastName} ({x.emp.EmployeeCode})",
    //                DepartmentName = x.dep.DepartmentName
    //            }).Distinct().AsNoTracking().ToListAsync();

    //        return result;
    //    }
    //    #endregion


    //    #region GetShiftByCompany
    //    public async Task<List<RosterInOfficeDaysSetupVM>> GetShiftByOrganization(int? id)
    //    {
    //        var query = from sft in _shiftsRepository.AllActive().AsNoTracking()

    //                    join org in _organizationRepository.AllActive().AsNoTracking() on sft.OrganizationID equals org.OrganizationID into orgGroup
    //                    from org in orgGroup.DefaultIfEmpty()

    //                    select new { sft, org };

    //        if(id.HasValue && id.Value != 0)
    //        {
    //            query = query.Where(x => x.sft.OrganizationID == id.Value);
    //        }

    //        var result = await query
    //            .Select(x => new RosterInOfficeDaysSetupVM
    //            {
    //                ShiftID = x.sft.ShiftID,
    //                ShiftName = $"{x.sft.ShiftName} ({x.sft.StartTime} - {x.sft.EndTime})"
    //            }).AsNoTracking().ToListAsync();
                              
    //        return result;
    //    }
    //    #endregion
    //}
}
