using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.GeometriesGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
                if (model.OrganizationID != null && model.DepartmentIDs == null && model.EmployeeIDs == null)
                {
                    var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID);
                    //if (employees == null || !employees.Any())
                    //    continue;
                    foreach (var employee in employees)
                    {
                        if (model.ExcludedEmployeeIDs != null && model.ExcludedEmployeeIDs.Contains(employee.EmployeeID ?? 0))
                            continue;

                        var existingEntity = await _genericRepository.AllActive()
                            .Where(x => x.EmployeeID == employee.EmployeeID).FirstOrDefaultAsync();
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
                            DefaultShifts entity = new DefaultShifts();
                            entity.ShiftID = model.ShiftID;
                            entity.EmployeeID = employee.EmployeeID;
                            entity.LIP = model.LIP;
                            entity.LMAC = model.LMAC;
                            entity.CreatedBy = model.CreatedBy;
                            entity.CreatedAt = DateTime.Now;
                            await _genericRepository.AddAsync(entity);
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
                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.AllActive().Where(x => x.EmployeeID == employee.EmployeeID).FirstOrDefaultAsync();
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
                                DefaultShifts entity = new DefaultShifts();
                                entity.ShiftID = model.ShiftID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                await _genericRepository.AddAsync(entity);
                            }
                        }
                    }
                }
                else if (model.EmployeeIDs != null && model.EmployeeIDs.Any())
                {
                    foreach (var empId in model.EmployeeIDs)
                    {
                        var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID && x.EmployeeID == empId);
                        if (employees == null || !employees.Any())
                            continue;

                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.All().Where(x => x.EmployeeID == employee.EmployeeID).FirstOrDefaultAsync();

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
                                DefaultShifts entity = new DefaultShifts();
                                entity.ShiftID = model.ShiftID;
                                entity.EmployeeID = empId;

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


        #region CheckConflictsAsync
        public async Task<List<ConflictViewModel>> CheckConflictsAsync(AssignDefaultShiftSetupVM model)
        {
            var conflicts = new List<ConflictViewModel>();

            if (model.OrganizationID != null && model.DepartmentIDs == null && model.EmployeeIDs == null)
            {
                var employees = await _employeeOfficeInfo.FindAsync(x => x.OrganizationID == model.OrganizationID);

                foreach (var employee in employees)
                {
                    var existingEntity = await _genericRepository.All()
                        .Where(x => x.EmployeeID == employee.EmployeeID)
                        .Include(x => x.Shift)
                        .Include(x => x.Employee)
                        .ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                        .ThenInclude(x => x.Organization)
                        .Include(x => x.Employee)
                        .ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                        .ThenInclude(x => x.Department)
                        .FirstOrDefaultAsync();

                    if (existingEntity != null)
                    {
                        conflicts.Add(new ConflictViewModel
                        {
                            DefaultShiftID = existingEntity.DefaultShiftID,
                            OrganizationID = employee.OrganizationID,
                            OrganizationName = existingEntity.Employee?.EmployeeOfficeInfoEmployee.Select(x => x.Organization?.OrganizationName).FirstOrDefault(),
                            DepartmentName = existingEntity.Employee?.EmployeeOfficeInfoEmployee.Select(x => x.Department?.DepartmentName).FirstOrDefault(),
                            EmployeeID = employee.EmployeeID ?? 0,
                            EmployeeName = $"{existingEntity.Employee?.FirstName} {existingEntity.Employee?.LastName}" ?? "",
                            ShiftID = existingEntity.ShiftID,
                            ShiftName = existingEntity.Shift?.ShiftName ?? ""
                        });
                    }
                }
            }

            if (!conflicts.Any())
                return conflicts;

            // ✅ Step 1: Group by ShiftID and count how many employees have each shift
            var shiftCountMap = conflicts.GroupBy(c => c.ShiftID).ToDictionary(g => g.Key, g => g.Count());

            // ✅ Step 2: Find the max count (most frequently assigned shift)
            var maxCount = shiftCountMap.Values.Max();

            // ✅ Step 3: Get the ShiftIDs that have the max count
            var mostFrequentShiftIds = shiftCountMap.Where(kvp => kvp.Value == maxCount).Select(kvp => kvp.Key).ToHashSet();

            // ✅ Step 4: Filter out those employees who are on the most common shift(s)
            var filteredConflicts = conflicts.Where(c => !mostFrequentShiftIds.Contains(c.ShiftID)).ToList();

            return filteredConflicts;
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateAsync(AssignDefaultShiftSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.DefaultShiftID);
                if (entity == null)
                {
                    return false;
                }

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
        public async Task<bool> UpdateEmpShiftAsync(AssignDefaultShiftSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.DefaultShiftID);
                if (entity == null)
                {
                    return false;
                }

                entity.EmployeeID = model.EmployeeIDs?.FirstOrDefault();
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
        public async Task<PaginationService<DefaultShifts, AssignDefaultShiftSetupVM>.PaginationResult<AssignDefaultShiftSetupVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DefaultShiftID", string sortOrder = "desc", int? organizationID = null)
        {
            try
            {
                var query = _genericRepository.All()
                            .AsNoTracking()
                            .Include(x => x.Shift)
                            .Include(x => x.Employee)
                            .ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                            .ThenInclude(x => x.Organization)
                            .Include(x => x.Employee)
                            .ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                            .ThenInclude(x => x.Department)
                            .Where(x => x.DeletedAt == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "DefaultShiftID" => sortOrder == "desc" ? query.OrderByDescending(x => x.DefaultShiftID) : query.OrderBy(x => x.DefaultShiftID),
                        "ShiftName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Shift.ShiftName) : query.OrderBy(x => x.Shift.ShiftName),
                        "OrganizationName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Organization.OrganizationName)
                            : query.OrderBy(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Organization.OrganizationName),
                        "DepartmentName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName)
                            : query.OrderBy(x => x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName),
                        "EmployeeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Employee.FirstName) : query.OrderBy(x => x.Employee.FirstName),
                        _ => query.OrderBy(x => x.ShiftID)
                    };
                }

                if (pageSize == 0)
                {
                    pageSize = await query.CountAsync();
                    pageNumber = 1;
                }

                var result = await PaginationService<DefaultShifts, AssignDefaultShiftSetupVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.Shift.ShiftName, $"%{term}%")
                    || EF.Functions.Like(x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Organization.OrganizationName, $"%{term}%")
                    || EF.Functions.Like(x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName, $"%{term}%")
                    || EF.Functions.Like(x.Employee.FirstName, $"%{term}%"),
                    x => new AssignDefaultShiftSetupVM
                    {
                        DefaultShiftID = x.DefaultShiftID,
                        OrganizationName = x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Organization?.OrganizationName ?? "-",
                        DepartmentName = x.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Department?.DepartmentName ?? "-",
                        EmployeeName = $"{x.Employee?.FirstName} {x.Employee?.LastName} ({x.Employee?.EmployeeCode})",
                        ShiftName = x.Shift?.ShiftName ?? "-",
                    });

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        #region Separate
        //public async Task<PaginationResultVM<DefaultShiftListVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DefaultShiftID", string sortOrder = "desc")
        //{
        //    try
        //    {
        //        var query =
        //            from ds in _genericRepository.AllActive().AsNoTracking()

        //            join sft in _shiftsRepository.AllActive().AsNoTracking()
        //                on ds.ShiftID equals sft.ShiftID into sftJoin
        //            from sft in sftJoin.DefaultIfEmpty()

        //            join emp in _employeesRepository.AllActive().AsNoTracking()
        //                on ds.EmployeeID equals emp.EmployeeID into empJoin
        //            from emp in empJoin.DefaultIfEmpty()

        //            join eoi in _employeeOfficeInfo.AllActive().AsNoTracking()
        //                on emp.EmployeeID equals eoi.EmployeeID into eoiJoin
        //            from eoi in eoiJoin.DefaultIfEmpty()

        //            join org in _organizationRepository.AllActive().AsNoTracking()
        //                on eoi.OrganizationID equals org.OrganizationID into orgJoin
        //            from org in orgJoin.DefaultIfEmpty()

        //            join dep in _departmentRepository.AllActive().AsNoTracking()
        //                on eoi.DepartmentID equals dep.DepartmentID into depJoin
        //            from dep in depJoin.DefaultIfEmpty()

        //            select new DefaultShiftListVM
        //            {
        //                DefaultShiftID = ds.DefaultShiftID,
        //                ShiftName = sft.ShiftName ?? "-",
        //                EmployeeName = emp != null ? emp.FirstName + " " + emp.LastName : "-",
        //                EmployeeCode = emp.EmployeeCode ?? "-",
        //                OrganizationName = org.OrganizationName ?? "-",
        //                DepartmentName = dep.DepartmentName ?? "-"
        //            };



        //        if (!string.IsNullOrWhiteSpace(searchTerm))
        //        {
        //            searchTerm = searchTerm.Trim().ToLower();

        //            query = query.Where(x =>
        //                EF.Functions.Like(x.ShiftName.ToLower(), $"%{searchTerm}%") ||
        //                EF.Functions.Like(x.EmployeeName.ToLower(), $"%{searchTerm}%") ||
        //                EF.Functions.Like(x.OrganizationName.ToLower(), $"%{searchTerm}%") ||
        //                EF.Functions.Like(x.DepartmentName.ToLower(), $"%{searchTerm}%")
        //            );
        //        }

        //        var sortMap = new Dictionary<string, Expression<Func<DefaultShiftListVM, object>>>
        //        {
        //            { "DefaultShiftID", x => x.DefaultShiftID },
        //            { "ShiftName", x => x.ShiftName },
        //            { "EmployeeName", x => x.EmployeeName },
        //            { "OrganizationName", x => x.OrganizationName },
        //            { "DepartmentName", x => x.DepartmentName }
        //        };

        //        if (sortMap.ContainsKey(sortColumn))
        //        {
        //            var sortExpr = sortMap[sortColumn];

        //            query = sortOrder == "desc" ? query.OrderByDescending(sortExpr) : query.OrderBy(sortExpr);
        //        }
        //        else
        //        {
        //            query = query.OrderByDescending(x => x.DefaultShiftID);
        //        }

        //        //query = (sortColumn, sortOrder.ToLower()) switch
        //        //{
        //        //    ("ShiftName", "asc") => query.OrderBy(x => x.ShiftName),
        //        //    ("ShiftName", "desc") => query.OrderByDescending(x => x.ShiftName),
        //        //    ("EmployeeName", "asc") => query.OrderBy(x => x.EmployeeName),
        //        //    ("EmployeeName", "desc") => query.OrderByDescending(x => x.EmployeeName),
        //        //    ("OrganizationName", "asc") => query.OrderBy(x => x.OrganizationName),
        //        //    ("OrganizationName", "desc") => query.OrderByDescending(x => x.OrganizationName),
        //        //    ("DepartmentName", "asc") => query.OrderBy(x => x.DepartmentName),
        //        //    ("DepartmentName", "desc") => query.OrderByDescending(x => x.DepartmentName),
        //        //    ("DefaultShiftID", "asc") => query.OrderBy(x => x.DefaultShiftID),
        //        //    _ => query.OrderByDescending(x => x.DefaultShiftID)
        //        //};

        //        int totalItems = await query.CountAsync();

        //        if (pageSize == 0)
        //        {
        //            pageSize = totalItems;
        //            pageNumber = 1;
        //        }

        //        var data = await query
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        //        var pagination = new PaginationInfoVM
        //        {
        //            TotalItems = totalItems,
        //            TotalPages = totalPages,
        //            CurrentPage = pageNumber,
        //            StartItem = totalItems == 0 ? 0 : (pageNumber - 1) * pageSize + 1,
        //            EndItem = Math.Min(pageNumber * pageSize, totalItems),
        //            PageNumbers = Enumerable.Range(1, totalPages).ToList()
        //        };

        //        return new PaginationResultVM<DefaultShiftListVM>
        //        {
        //            Data = data,
        //            PaginationInfo = pagination,
        //            TotalCount = totalItems
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message, ex);
        //    }
        //}
        #endregion

        #endregion


        #region GetByIdAsync
        public async Task<AssignDefaultShiftSetupVM> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.AllActive()
                .Include(x => x.Employee)
                .ThenInclude(x => x.EmployeeOfficeInfoEmployee)
                .ThenInclude(x => x.Organization)
                .ThenInclude(x => x.Departments)
                .FirstOrDefaultAsync(x => x.DefaultShiftID == id);
            var defaultShift = entity as DefaultShifts;

            if (defaultShift == null)
                return null;

            return new AssignDefaultShiftSetupVM
            {
                DefaultShiftID = defaultShift.DefaultShiftID,
                ShiftID = defaultShift.ShiftID,
                OrganizationID = defaultShift.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().OrganizationID,
                DepartmentID = defaultShift.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().DepartmentID,
                EmployeeID = defaultShift.EmployeeID
            };
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