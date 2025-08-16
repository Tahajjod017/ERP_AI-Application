using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternService : AppService<SpiralPatternAssignList>, IAssignSpiralPatternService
    {
        #region Repositories
        private readonly IGenericRepository<SpiralPatternAssignList> _genericRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<SpiralPatternTypes> _spiralPatternTypeRepository;
        private readonly IGenericRepository<SpiralWeeklyPattern> _spiralWeeklyPatternRepository;
        private readonly IGenericRepository<SpiralBioWeeklyPattern> _spiralBioWeeklyPatternRepository;
        private readonly IGenericRepository<SpiralMonthlyPattern> _spiralMonthlyPatternRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;
        private readonly IGenericRepository<Departments> _departments;

        public AssignSpiralPatternService(IGenericRepository<SpiralPatternAssignList> genericRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<SpiralPatternTypes> spiralPatternTypeRepository, IGenericRepository<SpiralWeeklyPattern> spiralWeeklyPatternRepository, IGenericRepository<SpiralBioWeeklyPattern> spiralBioWeeklyPatternRepository, IGenericRepository<SpiralMonthlyPattern> spiralMonthlyPatternRepository, IGenericRepository<Data.Models.Employees> employeesRepository, IGenericRepository<Departments> departments) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            _organizationRepository = organizationRepository;
            _spiralPatternTypeRepository = spiralPatternTypeRepository;
            _spiralWeeklyPatternRepository = spiralWeeklyPatternRepository;
            _spiralBioWeeklyPatternRepository = spiralBioWeeklyPatternRepository;
            _spiralMonthlyPatternRepository = spiralMonthlyPatternRepository;
            _employeesRepository = employeesRepository;
            _departments = departments;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(AssignSpiralPatternSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if(model.OrganizationID != 0 && model.DepartmentIDs == null && model.EmployeeIDs == null)
                {
                    var employees = await _employeeOfficeInfoRepository.FindAsync(e => e.OrganizationID == model.OrganizationID);

                    foreach (var employee in employees)
                    {
                        var existingEntity = await _genericRepository.All()
                            .Where(e => e.OrganizationID == employee.OrganizationID 
                            && e.DepartmentID == employee.DepartmentID
                            && e.EmployeeID == employee.EmployeeID
                            && e.StartDate == model.StartDate 
                            && e.EndDate == model.EndDate
                            && e.DeletedAt != null && e.DeletedBy != null)
                            .FirstOrDefaultAsync();
                        if (existingEntity != null)
                        {
                            SpiralPatternAssignList entity = new SpiralPatternAssignList();
                            entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                            entity.SpiralPatternID = model.SpiralPatternID;
                            entity.CreatedBy = model.CreatedBy;
                            entity.CreatedAt = DateTime.Now;
                            entity.LIP = model.LIP;
                            entity.LMAC = model.LMAC;
                            entity.DeletedAt = null;
                            entity.DeletedBy = null;
                        }
                        else
                        {
                            SpiralPatternAssignList entity = new SpiralPatternAssignList();
                            entity.OrganizationID = model.OrganizationID;
                            entity.DepartmentID = employee.DepartmentID;
                            entity.EmployeeID = employee.EmployeeID;
                            entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                            entity.SpiralPatternID = model.SpiralPatternID;
                            entity.StartDate = model.StartDate;
                            entity.EndDate = model.EndDate;
                            entity.CreatedBy = model.CreatedBy;
                            entity.CreatedAt = DateTime.Now;
                            entity.LIP = model.LIP;
                            entity.LMAC = model.LMAC;
                            
                            await _genericRepository.AddAsync(entity);
                        }
                    }
                }
                else if (model.OrganizationID != null && model.DepartmentIDs != null && model.EmployeeIDs == null)
                {
                    foreach(var depId in model.DepartmentIDs)
                    {
                        var employees = await _employeeOfficeInfoRepository.FindAsync(e => e.OrganizationID == model.OrganizationID && e.DepartmentID == depId);
                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(e => e.OrganizationID == employee.OrganizationID
                                && e.DepartmentID == employee.DepartmentID
                                && e.EmployeeID == employee.EmployeeID
                                && e.StartDate == model.StartDate
                                && e.EndDate == model.EndDate
                                && e.DeletedAt != null && e.DeletedBy != null)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                entity.DeletedAt = null;
                                entity.DeletedBy = null;
                            }
                            else
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.OrganizationID = model.OrganizationID;
                                entity.DepartmentID = employee.DepartmentID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.StartDate = model.StartDate;
                                entity.EndDate = model.EndDate;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                await _genericRepository.AddAsync(entity);
                            }
                        }
                    }
                }
                else if (model.EmployeeIDs != null)
                {
                    foreach (var empId in model.EmployeeIDs)
                    {
                        var employee = (await _employeeOfficeInfoRepository.FindAsync(e => e.EmployeeID == empId)).FirstOrDefault();
                        if (employee != null)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(e => e.OrganizationID == employee.OrganizationID
                                && e.DepartmentID == employee.DepartmentID
                                && e.EmployeeID == employee.EmployeeID
                                && e.StartDate == model.StartDate
                                && e.EndDate == model.EndDate
                                && e.DeletedAt != null && e.DeletedBy != null)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                entity.DeletedAt = null;
                                entity.DeletedBy = null;
                            }
                            else
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.OrganizationID = model.OrganizationID;
                                entity.DepartmentID = employee.DepartmentID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.StartDate = model.StartDate;
                                entity.EndDate = model.EndDate;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
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


        #region GetByIdAsync
        public async Task<AssignSpiralPatternEditVM> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _genericRepository.GetByIdAsync(id);

                if (entity == null)
                {
                    return null;
                }

                return new AssignSpiralPatternEditVM
                {
                    SpiralPatternAssignListID = entity.SpiralPatternAssignListID,
                    EditOrganizationID = entity.OrganizationID,
                    EditDepartmentID = entity.DepartmentID,
                    EditEmployeeID = entity.EmployeeID,
                    EditSpiralPatternTypeID = entity.SpiralPatternTypeID,
                    EditSpiralPatternID = entity.SpiralPatternID,
                    EditStartDate = entity.StartDate,
                    EditEndDate = entity.EndDate
                };
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<SeparatePaginationResult<AssignSpiralPatternListVM>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralPatternAssignListID",
            string sortOrder = "desc")
        {
            var query = from spa in _genericRepository.AllActive().AsNoTracking()

                        join eoi in _employeeOfficeInfoRepository.All().AsNoTracking() on spa.EmployeeID equals eoi.EmployeeID into eoiGroup
                        from eoi in eoiGroup.DefaultIfEmpty()

                        join emp in _employeesRepository.All().AsNoTracking() on eoi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()
                        
                        join org in _organizationRepository.All().AsNoTracking() on spa.OrganizationID equals org.OrganizationID into orgGroup
                        from org in orgGroup.DefaultIfEmpty()

                        join dep in _departments.All().AsNoTracking() on spa.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()

                        join spt in _spiralPatternTypeRepository.All().AsNoTracking() on spa.SpiralPatternTypeID equals spt.SpiralPatternTypeID into sptGroup
                        from spt in sptGroup.DefaultIfEmpty()
                        
                        join swp in _spiralWeeklyPatternRepository.All().AsNoTracking() on spa.SpiralPatternID equals swp.SpiralWeeklyPatternID into swpGroup
                        from swp in swpGroup.DefaultIfEmpty()
                        
                        join sbp in _spiralBioWeeklyPatternRepository.All().AsNoTracking() on spa.SpiralPatternID equals sbp.SpiralBioWeeklyPatternID into sbpGroup
                        from sbp in sbpGroup.DefaultIfEmpty()
                        
                        join smp in _spiralMonthlyPatternRepository.All().AsNoTracking() on spa.SpiralPatternID equals smp.SpiralMonthlyPatternID into smpGroup
                        from smp in smpGroup.DefaultIfEmpty()
                        select new AssignSpiralPatternListVM
                        {
                            SpiralPatternAssignListID = spa.SpiralPatternAssignListID,
                            OrganizationID = spa.OrganizationID,
                            OrganizationName = org.OrganizationName ?? "-",
                            DepartmentID = spa.DepartmentID,
                            DepartmentName = dep.DepartmentName ?? "-",
                            EmployeeID = spa.EmployeeID,
                            EmployeeName = $"{emp.FirstName} {emp.LastName}",
                            SpiralPatternTypeID = spa.SpiralPatternTypeID,
                            SpiralPatternTypeName = spt.SpiralPatternTypeName ?? "-",
                            SpiralPatternID = spa.SpiralPatternID,
                            SpiralPatternName =
                                spa.SpiralPatternTypeID == 1 ? (swp.SpiralWeeklyPatternName ?? "-") :
                                spa.SpiralPatternTypeID == 2 ? (sbp.SpiralBioWeeklyPatternName ?? "-") :
                                spa.SpiralPatternTypeID == 3 ? (smp.SpiralMonthlyPatternName ?? "-") :
                                "-",
                            StartDate = spa.StartDate.HasValue ? spa.StartDate.Value.ToString("dd/MM/yyyy") : "-",
                            EndDate = spa.EndDate.HasValue ? spa.EndDate.Value.ToString("dd/MM/yyyy") : "-"
                        };

            // Apply search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    x.OrganizationName.Contains(searchTerm) ||
                    x.SpiralPatternTypeName.Contains(searchTerm) ||
                    x.SpiralPatternName.Contains(searchTerm));
            }

            // Total records
            var totalCount = await query.CountAsync();

            // Sorting
            switch (sortColumn)
            {
                case "OrganizationName":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.OrganizationName) : query.OrderByDescending(x => x.OrganizationName);
                    break;

                case "EmployeeName":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.EmployeeName) : query.OrderByDescending(x => x.EmployeeName);
                    break;

                case "SpiralPatternTypeName":
                    query = sortOrder.ToLower() == "asc"? query.OrderBy(x => x.SpiralPatternTypeName) : query.OrderByDescending(x => x.SpiralPatternTypeName);
                    break;

                case "SpiralPatternName":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.SpiralPatternName) : query.OrderByDescending(x => x.SpiralPatternName);
                    break;

                case "StartDate":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate);
                    break;

                case "EndDate":
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate);
                    break;

                case "SpiralPatternAssignListID":
                default:
                    query = sortOrder.ToLower() == "asc" ? query.OrderBy(x => x.SpiralPatternAssignListID) : query.OrderByDescending(x => x.SpiralPatternAssignListID);
                    break;
            }

            var items = pageSize == 0
                    ? await query.ToListAsync()
                    : await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var startItem = pageSize == 0 ? 1 : ((pageNumber - 1) * pageSize + 1);
            var endItem = pageSize == 0 ? totalCount : Math.Min(startItem + pageSize - 1, totalCount);
            var totalPages = pageSize == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);

            var paginationInfo = new SeparatePaginationInfo
            {
                StartItem = startItem,
                EndItem = endItem,
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageSize == 0 ? 1 : pageNumber,
                PageNumbers = Enumerable.Range(1, totalPages).ToList()
            };

            return new SeparatePaginationResult<AssignSpiralPatternListVM>
            {
                Data = items,
                TotalCount = totalCount,
                SeparatePaginationInfo = paginationInfo
            };
        }
        #endregion


        #region Get Spiral Weekly Patterns List
        public async Task<List<SpiralWeeklyPatternList>> GetAllSpiralWeeklyPatternAsync(int id)
        {
            var result = await _spiralWeeklyPatternRepository.AllActive()
                .Where(x => x.SpiralWeeklyPatternID == id)
                .Select(x => new SpiralWeeklyPatternList
                {
                    SpiralWeeklyPatternID = x.SpiralWeeklyPatternID,
                    SpiralPatternName = x.SpiralWeeklyPatternName,
                    OrganizationID = x.OrganizationID,
                    SpiralPatternTypeID = x.SpiralPatternTypeID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName : "-",
                    SpiralWeeklyPatternDetailsListVMs = x.SpiralWeeklyPatternDetails.Select(d => new SpiralWeeklyPatternDetailsListVM
                    {
                        SpiralWeeklyPatternDetailID = d.SpiralWeeklyPatternDetailID,
                        DayOfWeek = d.DayOfWeek,
                        ShiftID = d.ShiftID,
                        ShiftName = d.Shift != null ? d.Shift.ShiftName : "-",
                        ShiftTime = d.Shift != null ? $"{d.Shift.StartTime} - {d.Shift.EndTime}" : "-"
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }
        #endregion


        #region Get Spiral Fortnightly Patterns List
        public async Task<List<SpiralBioWeeklyPatternListVM>> GetAllSpiralFortnightlyPatternAsync(int id)
        {
            var result = await _spiralBioWeeklyPatternRepository.AllActive()
                .Where(x => x.SpiralBioWeeklyPatternID == id)
                .Select(x => new SpiralBioWeeklyPatternListVM
                {
                    SpiralBioWeeklyPatternID = x.SpiralBioWeeklyPatternID,
                    SpiralBioWeeklyPatternName = x.SpiralBioWeeklyPatternName,
                    OrganizationID = x.OrganizationID,
                    SpiralPatternTypeID = x.SpiralPatternTypeID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName : "-",
                    SpiralBioWeeklyPatternDetailsListVMs = x.SpiralBioWeeklyPatternDetails.Select(d => new SpiralBioWeeklyPatternDetailsListVM
                    {
                        SpiralBioWeeklyPatternDetailID = d.SpiralBioWeeklyPatternDetailID,
                        DayOfMonth = d.DayOfMonth,
                        ShiftID = d.ShiftID,
                        ShiftName = d.Shift != null ? d.Shift.ShiftName : "-",
                        ShiftTime = d.Shift != null ? $"{d.Shift.StartTime} - {d.Shift.EndTime}" : "-"
                    }).ToList()
                }).AsNoTracking().ToListAsync();

            return result;
        }
        #endregion


        #region Get Spiral Fortnightly Patterns List
        public async Task<List<SpiralMonthlyPatternListVM>> GetAllSpiralMonthlyPatternAsync(int id)
        {
            var rawData = await _spiralMonthlyPatternRepository.AllActive()
                .Where(x => x.SpiralMonthlyPatternID == id)
                .Select(x => new SpiralMonthlyPatternListVM
                {
                    SpiralMonthlyPatternID = x.SpiralMonthlyPatternID,
                    SpiralMonthlyPatternName = x.SpiralMonthlyPatternName,
                    OrganizationID = x.OrganizationID,
                    SpiralPatternTypeID = x.SpiralPatternTypeID,
                    OrganizationName = x.Organization != null ? x.Organization.OrganizationName : "-",
                    SpiralMonthlyPatternDetailsListVMs = x.SpiralMonthlyPatternDetails.Select(d => new SpiralMonthlyPatternDetailsListVM
                    {
                        SpiralMonthlyPatternDetailID = d.SpiralMonthlyPatternDetailID,
                        DayOfMonth = d.DayOfMonth,
                        ShiftID = d.ShiftID,
                        ShiftName = d.Shift != null ? d.Shift.ShiftName : "-",
                        ShiftTime = d.Shift != null ? $"{d.Shift.StartTime} - {d.Shift.EndTime}" : "-"
                    }).ToList()
                }).AsNoTracking().ToListAsync();

            return rawData;
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<AssignSpiralPatternDeleteVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.SpiralPatternAssignListID));
                if (data == null || data.Count == 0)
                {
                    return new AssignSpiralPatternDeleteVM
                    {
                        Message = "No data found to delete."
                    };
                }

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _genericRepository.CommitTransactionAsync();

                return new AssignSpiralPatternDeleteVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion
    }
}
