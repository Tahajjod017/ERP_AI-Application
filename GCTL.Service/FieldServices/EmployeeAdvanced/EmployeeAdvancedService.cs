using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NetTopologySuite.Precision;
using Newtonsoft.Json;

#region Services
namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public class EmployeeAdvancedService : AppService<EmployeeAdvances>, IEmployeeAdvanced
    {
        public readonly IGenericRepository<EmployeeAdvances> _genericRepository;
        public readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        public readonly IGenericRepository<GCTL.Data.Models.JobTypes> _jobtyperepository;
        public readonly IGenericRepository<EmployeeAdvanceFor> _employeeAdvanceForRepository;
        public readonly IGenericRepository<Customers> _customer;
        public readonly IGenericRepository<Jobs> _job;
        public readonly IGenericRepository<GroupEmployee> _groupEmployeeRepository;
        private readonly IUserInfoService _userInfoService;

        //
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        //

        public EmployeeAdvancedService(IGenericRepository<EmployeeAdvances> genericRepository, IGenericRepository<Data.Models.Employees> employees, IGenericRepository<JobTypes> jobtyperepository, IGenericRepository<EmployeeAdvanceFor> employeeAdvanceForRepository, IGenericRepository<Customers> customer, IGenericRepository<Jobs> job, IGenericRepository<GroupEmployee> groupEmployeeRepository, IUserInfoService userInfoService, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<ApprovalDesignation> approvaldesignation) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employees = employees;
            _jobtyperepository = jobtyperepository;
            _employeeAdvanceForRepository = employeeAdvanceForRepository;
            _customer = customer;
            _job = job;
            _groupEmployeeRepository = groupEmployeeRepository;
            _userInfoService = userInfoService;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.approvaldesignation = approvaldesignation;
        }
        #endregion
        #region GetAllAsync
        public Task<PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.PaginationResult<EmployeeAdvancedVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainempId = null)
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(e => e.EmployeeAdvanceFor)
                    .Include(e => e.Job).ThenInclude(e => e.Customer) // Job -> Customer
                    .Include(e => e.Job).ThenInclude(e => e.JobType)  // Job -> JobType
                    .Include(e => e.GroupEmployee).ThenInclude(e => e.Employee) // GroupEmployee -> Employee
                    .Include(e => e.ApprovalStatus)
                    .Include(e => e.RequestedByUser)


                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (mainempId != null)
                {
                    query = query.Where(x => x.EmployeeAdvanceID == mainempId);
                }

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "empId" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.Job.CustomerID)
                        : query.OrderBy(x => x.Job.CustomerID),

                        //"" => sortOrder == "desc"
                        //    ? query.OrderByDescending(x => x.MainAccount.Class.ClassName)
                        //    : query.OrderBy(x => x.MainAccount.Class.ClassName),


                        "empName" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.Job.Customer.FullName)
                        : query.OrderBy(x => x.Job.Customer.FullName),

                        "empProjectName" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Job.JobTitle)
                            : query.OrderBy(x => x.Job.JobTitle),

                        "empProjectType" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Job.JobType.JobTypeName)
                            : query.OrderBy(x => x.Job.JobType.JobTypeName),

                        "empSalary" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.AmountRequested)
                            : query.OrderBy(x => x.AmountRequested),

                        //"empGroupName" => sortOrder == "desc"
                        //    ? query.OrderByDescending(x => x.GroupEmployee.Count.ToString)
                        //    : query.OrderBy(x => x.GroupEmployee.Count.ToString),

                        "empStatus" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.ApprovalStatus.StatusName)
                            : query.OrderBy(x => x.ApprovalStatus.StatusName),

                        "empapprovedName" => sortOrder == "desc"
                                ? query.OrderByDescending(x => x.RequestedByUser.FirstName)
                                : query.OrderBy(x => x.RequestedByUser.FirstName),

                        "empDate" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate),
                        _ => query.OrderBy(x => x.EmployeeAdvanceID)
                    };
                }
                return PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    searchPredicate: (term) => x =>
                    x.EmployeeAdvanceID.ToString().ToLower().Contains(term) ||
                        x.Job.JobTitle.ToLower().Contains(term) ||
                        x.AmountRequested.ToString().ToLower().Contains(term) ||
                        x.Job.Customer.FullName.ToLower().Contains(term) ||
                        x.Job.JobType.JobTypeName.ToLower().Contains(term) ||
                        x.ApprovalStatus.StatusName.ToLower().Contains(term) ||
                        (
                        (x.RequestedByUser.FirstName ?? "").ToLower().Contains(term) ||
                        (x.RequestedByUser.LastName ?? "").ToLower().Contains(term)
                        ) || x.GroupEmployee.Any(ge =>
                                 ge.Employee.FirstName.ToLower().Contains(term) ||
                                 ge.Employee.LastName.ToLower().Contains(term)
                        ),

                    selector: x => new EmployeeAdvancedVM


                    {
                        EmployeeAdvanceID = x.EmployeeAdvanceID,
                        CustomerName = x.Job.Customer.FullName, // Job -> Customer then include
                        JobTypeName = x.Job.JobType.JobTypeName, // Job -> JobType -> JobTypeName

                        RequestedByUser = (x.RequestedByUser?.FirstName ?? "")
                + (string.IsNullOrEmpty(x.RequestedByUser?.LastName) ? "" : " " + x.RequestedByUser?.LastName) ?? "", // Concutination
                        CustomerID2 = x.Job.CustomerID,
                        JobID = x.JobID,
                        JobTitle = x.Job.JobTitle,
                        AmountRequested = x.AmountRequested,
                        GroupEmployeeID = x.GroupEmployee
                            .Select(ge => ge.EmployeeID)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList(),
                        GroupEmployeeName = x.GroupEmployee.Select(ge => ge.Employee.FirstName).ToList(), // GroupEmployee -> Employee -> FristName,LastNme
                        ApprovalStatusID = x.ApprovalStatusID,
                        StatusName = x.ApprovalStatus.StatusName,

                        StartDate = x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,

                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Add EmployeeAdvanced.", ex);
            }

        }
        #endregion
        #region Add
        // added by 404

        private async Task<int?> ResolveApprovalAsync(int? approvalId, bool isDesignation, dynamic offf)
        {
            if (!approvalId.HasValue) return null;

            if (isDesignation)
            {
                var code = await approvaldesignation.AllActive()
                    .Where(x => x.ApprovalDesignationID == approvalId)
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync();

                return code switch
                {
                    1 => offf.ImmediateSupervisorId,
                    2 => offf.SeniorSupervisorId,
                    3 => offf.HeadOfDepartmentId,
                    _ => null
                };
            }

            return approvalId;
        }
        private bool IsSelfApprovalBlocked(int? employeeId, int? approverId, bool? allowSelfApproval, int? exceptionId)
        {
            if (approverId != employeeId) return false;
            if (allowSelfApproval == true && exceptionId != employeeId) return false;
            return true;
        }
        //

        public async Task<CommonReturnViewModel> AddAsync(EmployeeAdvancedVM emp)
        {
            try
            {

                //

                var offf = await empoffi.AllActive()
                .Where(x => x.EmployeeID == emp.CreatedBy)
                .Select(x => new
                {
                    x.EmployeeID,
                    x.OrganizationID,
                    x.OrganizationBranchID,
                    x.SeniorSupervisorId,
                    x.ImmediateSupervisorId,
                    x.HeadOfDepartmentId
                }).FirstOrDefaultAsync();

                if (offf == null)
                    return new CommonReturnViewModel { Success = false, Message = "Employee office info not found." };

                var approvalSettings = await approvalSettingsRepository.AllActive()
                    .Include(x => x.ApprovalType).Where(x =>
                        x.OrganizationID == offf.OrganizationID &&
                        (x.OrganizationBranchID == null || x.OrganizationBranchID == offf.OrganizationBranchID) &&
                        x.ApprovalType.ApprovalTypeName == "Transport Advance Approval").FirstOrDefaultAsync();

                if (approvalSettings == null) return new CommonReturnViewModel { Success = false, Message = "No active Approval settings found." };

                int? approvalPersonId = null;

                var approvalFlow = new List<(int? id, bool isDesignation)>
        {
            (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
            (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
            (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
        };

                bool isSelfApprover = false;
                foreach (var step in approvalFlow)
                {
                    var resolvedId = await ResolveApprovalAsync(step.id, step.isDesignation, offf);
                    if (resolvedId == emp.CreatedBy)
                    {
                        isSelfApprover = true;
                        break;
                    }
                }


                // Step 2: Self-approval logic for known fixed levels
                if (isSelfApprover)
                {
                    if (approvalSettings.AllowSelfApproval == true)
                    {
                        if (emp.CreatedBy == approvalSettings.FirstApprovalID && emp.CreatedBy == approvalSettings.FirstApprovalID)
                        {
                            approvalPersonId = approvalSettings.SecondApprovalID;
                        }
                        else if (emp.CreatedBy == approvalSettings.SecondApprovalID && emp.CreatedBy == approvalSettings.SecondApprovalID)
                        {
                            approvalPersonId = approvalSettings.ThirdApprovalID;
                        }
                        else if (emp.CreatedBy == approvalSettings.ThirdApprovalID && emp.CreatedBy == approvalSettings.ThirdApprovalID)
                        {
                            approvalPersonId = approvalSettings.ThirdApprovalID;
                        }
                    }
                    else // Self-approval not allowed
                    {
                        approvalPersonId = approvalSettings.SelfExceptionApprovalID;
                    }

                    
                }

                // Step 3: Normal approval flow (fallback logic)
                foreach (var (id, isDesignation) in approvalFlow)
                {
                    var resolvedId = await ResolveApprovalAsync(id, isDesignation, offf);

                    // Skip if resolved is the applicant
                    if (resolvedId == emp.CreatedBy)
                    {
                        continue;
                    }

                    // Skip if blocked
                    if (resolvedId.HasValue &&
                        !IsSelfApprovalBlocked(emp.CreatedBy, resolvedId.Value,
                            approvalSettings.AllowSelfApproval, approvalSettings.SelfExceptionApprovalID))
                    {
                        approvalPersonId = resolvedId.Value;
                        break;
                    }
                }
                //

                await _genericRepository.BeginTransactionAsync();
                EmployeeAdvances empadvance = new EmployeeAdvances();

                //empadvance.EmployeeAdvanceID = emp.EmployeeAdvanceID;
                empadvance.JobID = emp.JobID;
                empadvance.AmountRequested = (decimal)emp.AmountRequested;
                empadvance.StartDate = emp.StartDate.HasValue ? DateOnly.FromDateTime(emp.StartDate.Value) : null;
                empadvance.EndDate = emp.EndDate.HasValue ? DateOnly.FromDateTime(emp.EndDate.Value) : null;
                empadvance.LIP = emp.LIP;
                empadvance.LMAC = emp.LMAC;
                empadvance.CreatedAt = DateTime.UtcNow;
                empadvance.CreatedBy = emp.CreatedBy;
                empadvance.UpdatedBy = emp.UpdatedBy;
                empadvance.ApprovedByUserID = approvalPersonId;   // comes from approvalsetings according to settings 
                empadvance.ApprovalStatusID = 11; // Pending



                //empadvance.JobID = emp.JobID;
                await _genericRepository.AddAsync(empadvance);

                //(Multiple JobType Save to EmployeeAdvanceFor Table) 
                if (emp.RequestedByUserID != null)
                {
                    foreach (var item in emp.RequestedByUserID)
                    {
                        EmployeeAdvanceFor employeeAdvanceFor = new EmployeeAdvanceFor();
                        employeeAdvanceFor.EmployeeAdvanceID = empadvance.EmployeeAdvanceID;
                        employeeAdvanceFor.JobTypeID = item;
                        employeeAdvanceFor.LIP = emp.LIP;
                        employeeAdvanceFor.LMAC = emp.LMAC;
                        employeeAdvanceFor.CreatedAt = DateTime.Now;
                        employeeAdvanceFor.CreatedBy = emp.CreatedBy;

                        await _employeeAdvanceForRepository.AddAsync(employeeAdvanceFor);

                    }
                    // Multiple Grop Employee Save to GroupEmployee Table
                    if (emp.GroupEmployeeID != null)
                    {
                        foreach (var item in emp.GroupEmployeeID)
                        {
                            GroupEmployee groupEmployee = new GroupEmployee();
                            groupEmployee.EmployeeAdvanceID = empadvance.EmployeeAdvanceID;
                            groupEmployee.EmployeeID = item;
                            groupEmployee.LIP = emp.LIP;
                            groupEmployee.LMAC = emp.LMAC;
                            groupEmployee.CreatedAt = DateTime.Now;
                            groupEmployee.CreatedBy = emp.CreatedBy;

                            await _groupEmployeeRepository.AddAsync(groupEmployee);


                        }
                    }

                }

                await _genericRepository.CommitTransactionAsync();    
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Advance Added Successfully"
                };



            }
            catch (Exception ex)
            {

                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

        }
        #endregion

        #region EmployeeDD

        //Modern Dropdown for Employees
        public async Task<IEnumerable<CommonSelectVM>> EmployeeDD()
        {
            var data = await _employees.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName} {x.EmployeeCode}",

                }).ToListAsync();
            return data;
        }
        #endregion

        #region GetJob Service
        public async Task<ReturnDataView<SelectListItem>> GetJobTypeAsync(string search, int page, int pageSize, int organizationID)
        {
            var query = _jobtyperepository.AllActive()
                .Where(q => q.OrganizationID == organizationID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.JobTypeID, pattern)
                    ));
            }
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new SelectListItem
                {

                    Value = t.JobTypeID.ToString(),
                    Text = t.JobTypeName,

                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
                totalItem = totalCount,
                message = "Data loaded"
            };
        }
        #endregion

        #region GetJobsByCusId(Nestesd)
        public async Task<List<EmployeeAdvancedVM>> GetJobByCusId(int customerId)
        {
            try
            {
                var data = await (from j in _job.AllActive()
                                  join c in _customer.AllActive() on j.CustomerID equals c.CustomerID
                                  where j.CustomerID == customerId
                                  select new EmployeeAdvancedVM
                                  {
                                      JobID = j.JobID,
                                      JobTitle = j.JobTitle + " " + (j.StartDateTime.HasValue ? j.StartDateTime.Value.ToString("dd/MM/yyyy") : string.Empty) + " - " + (j.EndDateTime.HasValue ? j.EndDateTime.Value.ToString("dd/MM/yyyy") : string.Empty)
                                  }).ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Approve Service
        public async Task<CommonReturnViewModel> ApproveAsync(int id, int approvedByUserId)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var empAdvance = await _genericRepository.GetByIdAsync(id);

                if (empAdvance == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Advance not found"
                    };
                }

                if (empAdvance.ApprovalStatusID == 12)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Advance is already approved"
                    };
                }

                empAdvance.ApprovalStatusID = 12; // Approved
                empAdvance.ApprovedByUserID = approvedByUserId;
                empAdvance.ApprovalDate = DateTime.UtcNow;
                empAdvance.UpdatedBy = approvedByUserId;
                empAdvance.UpdatedAt = DateTime.UtcNow;

                await _genericRepository.UpdateAsync(empAdvance);
                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Advance approved successfully"
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        //#region GetAllAsync
        //public Task<PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.PaginationResult<EmployeeAdvancedVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainempId = null)
        //{
        //    try
        //    {
        //        var query = _genericRepository.AllActive()
        //            .Include(e => e.EmployeeAdvanceFor)
        //            .Include(e => e.Job).ThenInclude(e => e.Customer) // Job -> Customer
        //            .Include(e => e.Job).ThenInclude(e => e.JobType)  // Job -> JobType
        //            .Include(e => e.GroupEmployee).ThenInclude(e => e.Employee) // GroupEmployee -> Employee
        //            .Include(e => e.ApprovalStatus)
        //            .Include(e => e.RequestedByUser)


        //            .AsNoTracking()
        //            .Where(x => x.DeletedAt == null && x.DeletedBy == null);

        //        if (mainempId != null)
        //        {
        //            query = query.Where(x => x.EmployeeAdvanceID == mainempId);
        //        }

        //        if (!string.IsNullOrEmpty(sortColumn))
        //        {
        //            query = sortColumn switch
        //            {
        //                "empId" => sortOrder == "desc"
        //                ? query.OrderByDescending(x => x.Job.CustomerID)
        //                : query.OrderBy(x => x.Job.CustomerID),

        //                //"" => sortOrder == "desc"
        //                //    ? query.OrderByDescending(x => x.MainAccount.Class.ClassName)
        //                //    : query.OrderBy(x => x.MainAccount.Class.ClassName),


        //                "empName" => sortOrder == "desc"
        //                ? query.OrderByDescending(x => x.Job.Customer.FullName)
        //                : query.OrderBy(x => x.Job.Customer.FullName),

        //                "empProjectName" => sortOrder == "desc"
        //                    ? query.OrderByDescending(x => x.Job.JobTitle)
        //                    : query.OrderBy(x => x.Job.JobTitle),

        //                "empProjectType" => sortOrder == "desc"
        //                    ? query.OrderByDescending(x => x.Job.JobType.JobTypeName)
        //                    : query.OrderBy(x => x.Job.JobType.JobTypeName),

        //                "empSalary" => sortOrder == "desc"
        //                    ? query.OrderByDescending(x => x.AmountRequested)
        //                    : query.OrderBy(x => x.AmountRequested),

        //                //"empGroupName" => sortOrder == "desc"
        //                //    ? query.OrderByDescending(x => x.GroupEmployee.Count.ToString)
        //                //    : query.OrderBy(x => x.GroupEmployee.Count.ToString),

        //                "empStatus" => sortOrder == "desc"
        //                    ? query.OrderByDescending(x => x.ApprovalStatus.StatusName)
        //                    : query.OrderBy(x => x.ApprovalStatus.StatusName),

        //                "empapprovedName" => sortOrder == "desc"
        //                        ? query.OrderByDescending(x => x.RequestedByUser.FirstName)
        //                        : query.OrderBy(x => x.RequestedByUser.FirstName),

        //                "empDate" => sortOrder == "desc"
        //                ? query.OrderByDescending(x => x.StartDate)
        //                : query.OrderBy(x => x.StartDate),
        //                _ => query.OrderBy(x => x.EmployeeAdvanceID)
        //            };
        //        }
        //        return PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.GetPaginatedData(
        //            query,
        //            pageNumber,
        //            pageSize,
        //            searchTerm,
        //            sortColumn,
        //            sortOrder,
        //            searchPredicate: (term) => x =>
        //            x.EmployeeAdvanceID.ToString().ToLower().Contains(term) ||
        //                x.Job.JobTitle.ToLower().Contains(term) ||
        //                x.AmountRequested.ToString().ToLower().Contains(term) ||
        //                x.Job.Customer.FullName.ToLower().Contains(term) ||
        //                x.Job.JobType.JobTypeName.ToLower().Contains(term) ||
        //                x.ApprovalStatus.StatusName.ToLower().Contains(term) ||
        //                (
        //                (x.RequestedByUser.FirstName ?? "").ToLower().Contains(term) ||
        //                (x.RequestedByUser.LastName ?? "").ToLower().Contains(term)
        //                ) || x.GroupEmployee.Any(ge =>
        //                         ge.Employee.FirstName.ToLower().Contains(term) ||
        //                         ge.Employee.LastName.ToLower().Contains(term)
        //                ),

        //            selector: x => new EmployeeAdvancedVM


        //            {
        //                EmployeeAdvanceID = x.EmployeeAdvanceID,
        //                CustomerName = x.Job.Customer.FullName, // Job -> Customer then include
        //                JobTypeName = x.Job.JobType.JobTypeName, // Job -> JobType -> JobTypeName


        //        //        RequestedByUser = (x.RequestedByUser?.FirstName ?? "")
        //        //+ (string.IsNullOrEmpty(x.RequestedByUser?.LastName) ? "" : " " + x.RequestedByUser?.LastName) ?? "", // If Null could be here


        ////                RequestedByUser = x.RequestedByUser != null

        ////? $"{x.RequestedByUser.FirstName ?? ""}{(!string.IsNullOrEmpty(x.RequestedByUser.LastName) ? " " + x.RequestedByUser.LastName : "")}".Trim()
        ////: null,





        //                RequestedByUser = (x.RequestedByUser?.FirstName ?? "")
        //        + (string.IsNullOrEmpty(x.RequestedByUser?.LastName) ? "" : " " + x.RequestedByUser?.LastName) ?? "", // Concutination

        //                CustomerID2 = x.Job.CustomerID,
        //                JobID = x.JobID,
        //                JobTitle = x.Job.JobTitle,
        //                AmountRequested = x.AmountRequested,
        //                GroupEmployeeID = x.GroupEmployee
        //                    .Select(ge => ge.EmployeeID)
        //                    .Where(id => id.HasValue)
        //                    .Select(id => id.Value)
        //                    .ToList(),
        //                GroupEmployeeName = x.GroupEmployee.Select(ge => ge.Employee.FirstName).ToList(), // GroupEmployee -> Employee -> FristName,LastNme
        //                ApprovalStatusID = x.ApprovalStatusID,
        //                StatusName = x.ApprovalStatus.StatusName,

        //                StartDate = x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,

        //            }
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred while retrieving Add EmployeeAdvanced.", ex);
        //    }

        //}
        //#endregion


        #region GetByID (Edit)
        public async Task<EmployeeAdvancedVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.AllActive()
                    .Include(x => x.EmployeeAdvanceFor)
                    .Include(x => x.GroupEmployee).ThenInclude(x => x.Employee)
                    .Include(x => x.Job).ThenInclude(x => x.Customer)
                    .FirstOrDefaultAsync(x => x.EmployeeAdvanceID == id);
                return new EmployeeAdvancedVM
                {
                    CustomerID2 = data.Job.CustomerID,
                    CustomerName = data.Job.Customer.FullName,
                    JobID = data.JobID,
                    JobName = data.Job.JobTitle,
                    AmountRequested = data.AmountRequested,
                    StartDate = data.StartDate.HasValue ? data.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    EndDate = data.EndDate.HasValue ? data.EndDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    GroupEmployeeID = data.GroupEmployee
                        .Where(ge => ge.EmployeeID.HasValue)
                        .Select(ge => ge.EmployeeID.Value)
                        .ToList(),

                    RequestedByUserID = data.EmployeeAdvanceFor

                  .Where(e => e.JobTypeID.HasValue)
                  .Select(e => e.JobTypeID.Value)
                  .ToList(),

                    ApprovedByUserID = data.ApprovedByUserID,
                    EmployeeAdvanceID = data.EmployeeAdvanceID
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Employee Advanced by ID.", ex);
            }
        }
        #endregion

        #region UpdateAsync
        public async Task<CommonReturnViewModel> UpdateAsync(EmployeeAdvancedVM emp)
        {
            await _genericRepository.BeginTransactionAsync();
            {
                try
                {
                    var entity = await _genericRepository.GetByIdAsync(emp.EmployeeAdvanceID);
                    if (entity == null)
                    {
                        return new CommonReturnViewModel
                        {
                            Success = false,
                            Message = "Data not found"
                        };
                    }

                    var beforeEntity = new EmployeeAdvancedVM
                    {
                        EmployeeAdvanceID = entity.EmployeeAdvanceID,
                        JobID = entity.JobID,
                        AmountRequested = entity.AmountRequested,
                        StartDate = entity.StartDate.HasValue ? entity.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        EndDate = entity.EndDate.HasValue ? entity.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        RequestedByUserID = entity.RequestedByUserID.HasValue
                                ? new List<int> { entity.RequestedByUserID.Value }
                                : new List<int>(),
                        ApprovedByUserID = entity.ApprovedByUserID,
                        LIP = entity.LIP,
                        LMAC = entity.LMAC
                    };

                    // Update the GroupEmployee Multiple

                    if (emp.GroupEmployeeID != null)
                    {
                        foreach (var groupEmployee in entity.GroupEmployee)
                        {
                            if (emp.GroupEmployeeID.Contains(groupEmployee.EmployeeID ?? 0))
                            {
                                groupEmployee.EmployeeAdvanceID = emp.EmployeeAdvanceID;
                                groupEmployee.UpdatedAt = DateTime.UtcNow;
                                groupEmployee.UpdatedBy = emp.UpdatedBy;
                            }
                        }
                    }

                    entity.LIP = emp.LIP;
                    entity.LMAC = emp.LMAC;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = emp.UpdatedBy;

                    await _genericRepository.UpdateAsync(entity);

                    var afterEntity = new EmployeeAdvancedVM
                    {
                        EmployeeAdvanceID = entity.EmployeeAdvanceID,
                        JobID = entity.JobID,
                        AmountRequested = entity.AmountRequested,
                        StartDate = entity.StartDate.HasValue ? entity.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        EndDate = entity.EndDate.HasValue ? entity.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        RequestedByUserID = entity.RequestedByUserID.HasValue
                ? new List<int> { entity.RequestedByUserID.Value }
                : new List<int>(),
                        ApprovedByUserID = entity.ApprovedByUserID,
                        LIP = entity.LIP,
                        LMAC = entity.LMAC
                    };

                    await _userInfoService.ActionLogAsync("EmployeeAdvances", ActionName.DataUpdated, beforeEntity, afterEntity, entity.EmployeeAdvanceID, emp);

                    await _genericRepository.CommitTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee Advance Updated Successfully"
                    };
                }
                catch (Exception ex)
                {
                    await _genericRepository.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
        }
        #endregion

        #region Soft Delete
        public async Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.EmployeeAdvanceID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<CommonReturnViewModel>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.EmployeeAdvanceID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Add EmployeeAdvances", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Add Sub Account.", ex);
            }
        }
        #endregion





    }
}
