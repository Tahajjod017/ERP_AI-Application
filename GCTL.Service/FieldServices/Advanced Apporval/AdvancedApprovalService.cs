using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.FieldServices.EmployeeAdvanced;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.FieldServices.Advanced_Apporval
{
    public class AdvancedApprovalService : AppService<EmployeeAdvances>, IAdvancedApprovalService
    {
        public readonly IGenericRepository<EmployeeAdvances> _genericRepository;
        public readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        public readonly IGenericRepository<GCTL.Data.Models.JobTypes> _jobtyperepository;
        public readonly IGenericRepository<EmployeeAdvanceFor> _employeeAdvanceForRepository;
        public readonly IGenericRepository<Customers> _customer;
        public readonly IGenericRepository<Jobs> _job;
        public readonly IGenericRepository<GroupEmployee> _groupEmployeeRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly AppDbContext appDb;

        public AdvancedApprovalService(
            IGenericRepository<EmployeeAdvances> genericRepository,
            IGenericRepository<Data.Models.Employees> employees,
            IGenericRepository<JobTypes> jobtyperepository,
            IGenericRepository<EmployeeAdvanceFor> employeeAdvanceForRepository,
            IGenericRepository<Customers> customer,
            IGenericRepository<Jobs> job,
            IGenericRepository<GroupEmployee> groupEmployeeRepository,
            IUserInfoService userInfoService,
            AppDbContext appDb) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employees = employees;
            _jobtyperepository = jobtyperepository;
            _employeeAdvanceForRepository = employeeAdvanceForRepository;
            _customer = customer;
            _job = job;
            _groupEmployeeRepository = groupEmployeeRepository;
            _userInfoService = userInfoService;
            this.appDb = appDb;
        }

        public Task<CommonReturnViewModel> ApproveAsync(int employeeAdvanceID, string approvedBy)
        {
            throw new NotImplementedException();
        }

        public Task<CommonReturnViewModel> DeclineAsync(int employeeAdvanceID, string declinedBy)
        {
            throw new NotImplementedException();
        }

        // ====================================================================
        // BUG 1 FIX:
        // BEFORE: public Task<...> GetAllAsync1(...)          ← NOT async
        //         var employeeId = appDb.Users...FirstOrDefaultAsync();   ← Task<int?>, NOT int?
        //         var roleName   = (...).FirstOrDefaultAsync();           ← Task<string>, NOT string
        //
        // WHY IT WAS BROKEN:
        //   Without 'await', these variables hold a Task OBJECT, not the actual value.
        //   Any code using them gets garbage data or the wrong type entirely.
        //   This is a silent runtime bug — no compiler error, but wrong behavior.
        //
        // AFTER: async Task<...> + await on both database calls
        // ====================================================================

        #region GetAllAsync
        public async Task<PaginationService<EmployeeAdvances, ApprovalGetALLVM>.PaginationResult<ApprovalGetALLVM>> GetAllAsync1(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "EmployeeAdvanceID",
            string sortOrder = "desc",
            int? mainempId = null,
            string userId = "")
        {
            try
            {
                // FIX: Added 'await' — now employeeId is int? (the actual value), not Task<int?>
                var employeeId = await appDb.Users
                    .Where(u => u.Id == userId)
                    .Select(e => e.EmployeeId)
                    .FirstOrDefaultAsync();

                // FIX: Added 'await' — now roleName is string (the actual value), not Task<string>
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name).FirstOrDefaultAsync();

                // TODO: Uncomment and use these values when you implement role-based filtering
                // bool isSuperAdmin = string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
                // Example: if (!isSuperAdmin) query = query.Where(x => x.RequestedByUserID == employeeId);

                var query = _genericRepository.AllActive()
                    .Include(e => e.EmployeeAdvanceFor)
                    .Include(e => e.Job).ThenInclude(e => e.Customer)
                    .Include(e => e.Job).ThenInclude(e => e.JobType)
                    .Include(e => e.GroupEmployee).ThenInclude(e => e.Employee)
                    .Include(e => e.ApprovalStatus)
                    .Include(e => e.RequestedByUser)
                    .AsNoTracking();

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

                return await PaginationService<EmployeeAdvances, ApprovalGetALLVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    searchPredicate: (term) => x =>
                        x.Job.JobTitle.ToLower().Contains(term) ||
                        x.AmountRequested.ToString().ToLower().Contains(term),
                    selector: x => new ApprovalGetALLVM
                    {
                        EmployeeAdvanceID = x.EmployeeAdvanceID,
                        CustomerName = x.Job.Customer.FullName,
                        JobTypeName = x.Job.JobType.JobTypeName,
                        RequestedByUser = (x.RequestedByUser?.FirstName ?? "")
                            + (string.IsNullOrEmpty(x.RequestedByUser?.LastName)
                                ? ""
                                : " " + x.RequestedByUser?.LastName) ?? "",
                        CustomerID2 = x.Job.CustomerID,
                        JobID = x.JobID,
                        JobTitle = x.Job.JobTitle,
                        AmountRequested = x.AmountRequested,
                        GroupEmployeeID = x.GroupEmployee
                            .Select(ge => ge.EmployeeID)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList(),
                        GroupEmployeeName = x.GroupEmployee
                            .Select(ge => ge.Employee.FirstName)
                            .ToList(),
                        ApprovalStatusID = x.ApprovalStatusID,
                        StatusName = x.ApprovalStatus.StatusName,
                        StartDate = x.StartDate.HasValue
                            ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue)
                            : (DateTime?)null,
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving EmployeeAdvanced approvals.", ex);
            }
        }

        public Task<ApprovalGetALLVM> GetByIdAsync(int employeeAdvanceID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}