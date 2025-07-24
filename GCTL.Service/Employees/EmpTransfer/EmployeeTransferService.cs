using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Employees.EmpTransfer
{
    public class EmployeeTransferService : AppService<EmployeeTransfer>, IEmployeeTransferService
    {
        private readonly IGenericRepository<EmployeeTransfer> repositoryEmployeeTransfer;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        public EmployeeTransferService(IGenericRepository<EmployeeTransfer> genericRepository, IGenericRepository<EmployeeTransfer> repositoryEmployeeTransfer, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, AppDbContext appDb, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration, IGenericRepository<EmployeeOfficeInfo> empoffi) : base(genericRepository)
        {
            this.repositoryEmployeeTransfer = repositoryEmployeeTransfer;
            this.employee = employee;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.appDb = appDb;
            this.leavePolicyConfiguration = leavePolicyConfiguration;
            this.empoffi = empoffi;
        }

        #region Get All

        public async Task<PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null,
          List<int> departmentIds = null, List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name).FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = repositoryEmployeeTransfer.AllActive().Include(x => x.FromOrganization).Include(x => x.ToOrganization).Include(x => x.FromOrganizationBranch).Include(x => x.ToOrganizationBranch).OrderByDescending(x => x.EmployeeTransferID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }
               
                // 🔹 Step 4: Filter if not SuperAdmin
                //if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                //{
                //    query = query.Where(x => x.EmployeeID == employeeId);
                //}
                //
                //
                // Get all EmployeeOfficeInfo for filtering
                var officeInfoQuery = empoffi.AllActive().AsQueryable();

                //if (organizationId.HasValue)
                //{
                //    var empIds = await officeInfoQuery
                //        .Where(x => x.OrganizationID == organizationId)
                //        .Select(x => x.EmployeeID)
                //        .ToListAsync();

                //    query = query.Where(x => empIds.Contains(x.EmployeeID));
                //}

                //if (departmentIds?.Any() == true)
                //{
                //    var empIds = await officeInfoQuery
                //        .Where(x => departmentIds.Contains(x.DepartmentID ?? 0))
                //        .Select(x => x.EmployeeID)
                //        .ToListAsync();

                //    query = query.Where(x => empIds.Contains(x.EmployeeID));
                //}

                //if (employeeIds?.Any() == true)
                //{
                //    query = query.Where(x => employeeIds.Contains((int)x.EmployeeID));
                //}

                //
              
             
                //
                var result = await PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.EmployeeTransferID.ToString(), $"%{term}%"),
                     // EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                      //EF.Functions.Like(b.LeaveType.LeaveTypeName, $"%{term}%"),
                    


                    b => new EmployeeTransferTableListVM
                    {
                       
                      
                        EmployeeTransferID = b.EmployeeTransferID,
                      
                        TransferDate =DateTimeHelpers.FormatDateTime(b.TransferDate),
                     
                       // EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                       // EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
                       
                    });

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>
                {
                    Data = new List<EmployeeTransferTableListVM>(),
                    TotalCount = 0
                };
            }
        }

        #endregion


        #region Save Data
        public async Task<CommonReturnViewModel> SaveEmployeeTansferAsync(EmployeeTransferAddVM entityVM)
        {
           await repositoryEmployeeTransfer.BeginTransactionAsync();
            if (entityVM == null)
                return new CommonReturnViewModel { Success = false, Message = "Data cannot be null" };
            try
            {
                var entity = new EmployeeTransfer
                {
                    //EmployeeID=entityVM.EmployeeID,
                   FromOrganizationID = entityVM.FromOrganizationID,
                   ToOrganizationID = entityVM.ToOrganizationID,
                   FromOrganizationBranchID = entityVM.FromOrganizationBranchID,
                    ToOrganizationBranchID = entityVM.ToOrganizationBranchID,
                   TransferNote = entityVM.TransferNote,
                   TransferDate = entityVM.TransferDate,
                   LIP=entityVM.LIP,
                   LMAC=entityVM.LMAC,
                   CreatedAt=DateTime.Now,
                   CreatedBy=entityVM.CreatedBy,
                };
                await repositoryEmployeeTransfer.AddAsync(entity);
                await repositoryEmployeeTransfer.CommitTransactionAsync();
                return new CommonReturnViewModel { Success = true, Message="Saved Successfully"};
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
