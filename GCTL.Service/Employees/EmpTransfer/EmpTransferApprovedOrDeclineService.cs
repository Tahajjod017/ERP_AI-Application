using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Repository;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.Pagination;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using static Dapper.SqlMapper;

namespace GCTL.Service.Employees.EmpTransfer
{
    public class EmpTransferApprovedOrDeclineService:AppService<EmployeeTransfer>, IEmpTransferApprovedOrDeclineService
    {
        private readonly IGenericRepository<EmployeeTransfer> repositoryEmployeeTransfer;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<OrganizationBranches> organizationBranch;
        private readonly IGenericRepository<Alerts> alertsRepository;
        private readonly IGenericRepository<EmployeeTransferHistory> transferhistoryRepository;
        private readonly IGenericRepository<AlertForEmployee> alertForEmployeeRepository;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        private readonly IGenericRepository<Statuses> statusRepository;
        public EmpTransferApprovedOrDeclineService(IGenericRepository<EmployeeTransfer> genericRepository, IGenericRepository<EmployeeTransfer> repositoryEmployeeTransfer, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<OrganizationBranches> organizationBranch, IGenericRepository<Alerts> alertsRepository, IGenericRepository<EmployeeTransferHistory> transferhistoryRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IGenericRepository<Statuses> statusRepository = null) : base(genericRepository)
        {
            this.repositoryEmployeeTransfer = repositoryEmployeeTransfer;
            this.employee = employee;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.organizationBranch = organizationBranch;
            this.alertsRepository = alertsRepository;
            this.transferhistoryRepository = transferhistoryRepository;
            this.alertForEmployeeRepository = alertForEmployeeRepository;
            this.approvaldesignation = approvaldesignation;
            this.statusRepository = statusRepository;
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

                var query = repositoryEmployeeTransfer.AllActive().Where(x=>x.ApprovalPersonID==employeeId).Include(x => x.Employee)
             .Include(x => x.FromOrganization).Include(x => x.ToOrganization)
             .Include(x => x.FromOrganizationBranch).Include(x => x.ToOrganizationBranch).Include(x => x.FromDepartment)
             .Include(x => x.ToDepartment).Include(x => x.FromDesignation).Include(x => x.ToDesignation)
             .OrderByDescending(x => x.EmployeeTransferID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                // 🔹 Step 4: Filter if not SuperAdmin
                //if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                //{
                //    query = query.Where(x => x.EmployeeID == employeeId);
                //}
                


                // Get all EmployeeOfficeInfo for filtering
                var officeInfoQuery = empoffi.AllActive().AsQueryable();

                if (organizationId.HasValue)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => x.OrganizationID == organizationId)
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(x => empIds.Contains(x.EmployeeID));
                }

                if (departmentIds?.Any() == true)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => departmentIds.Contains(x.DepartmentID ?? 0))
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(x => empIds.Contains(x.EmployeeID));
                }

                if (employeeIds?.Any() == true)
                {
                    query = query.Where(x => employeeIds.Contains((int)x.EmployeeID));
                }
                if (!string.IsNullOrEmpty(currentSortColumn))
                {
                    switch (currentSortColumn.ToLower())
                    {
                        case "employeename":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName)
                                : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                            break;

                        case "fromorganization":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.FromOrganization.OrganizationName)
                                : query.OrderByDescending(x => x.FromOrganization.OrganizationName);
                            break;

                        case "toorganization":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.ToOrganization.OrganizationName)
                                : query.OrderByDescending(x => x.ToOrganization.OrganizationName);
                            break;

                        case "frombranch":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.FromOrganizationBranch.OrganizationBranchName)
                                : query.OrderByDescending(x => x.FromOrganizationBranch.OrganizationBranchName);
                            break;

                        case "tobranch":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.ToOrganizationBranch.OrganizationBranchName)
                                : query.OrderByDescending(x => x.ToOrganizationBranch.OrganizationBranchName);
                            break;

                        case "transferdate":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.TransferDate)
                                : query.OrderByDescending(x => x.TransferDate);
                            break;

                        default:
                            query = query.OrderByDescending(x => x.EmployeeTransferID);
                            break;
                    }
                }

                var result = await PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.GetPaginatedData(



                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                   term => b =>
                   EF.Functions.Like(b.EmployeeTransferID.ToString(), $"%{term}%") ||
                   EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                   EF.Functions.Like(b.FromOrganization.OrganizationName, $"%{term}%") ||
                   EF.Functions.Like(b.ToOrganization.OrganizationName, $"%{term}%") ||
                   EF.Functions.Like(b.FromOrganizationBranch.OrganizationBranchName, $"%{term}%") ||
                   EF.Functions.Like(b.ToOrganizationBranch.OrganizationBranchName, $"%{term}%"),
                   b => new EmployeeTransferTableListVM
                   {

                       EmployeeTransferID = b.EmployeeTransferID,
                       TransferDate = DateTimeHelpers.FormatDateTime(b.TransferDate),
                       FromOrganizationName = b.FromOrganization?.OrganizationName ?? "",
                       ToOrganizationName = b.ToOrganization?.OrganizationName ?? "",
                       FromOrganizationBranchName = b.FromOrganizationBranch?.OrganizationBranchName ?? "",
                       ToOrganizationBranchName = b.ToOrganizationBranch?.OrganizationBranchName ?? "",
                       FromDepartmentName = b.FromDepartment.DepartmentName,
                       ToDepartmentName = b.ToDepartment.DepartmentName,
                       FromDesignationName = b.FromDepartment.DepartmentName,
                       EmployeeName = b.Employee != null ? $"{b.Employee.FirstName ?? ""} {b.Employee.LastName ?? ""}".Trim() : "",
                       EmployeeImage = b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                       EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
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

        //Below Table

        public async Task<PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.PaginationResult<EmployeeTransferTableListVM>> GetAllTableListAsyncBelow(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null,
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

                var query = repositoryEmployeeTransfer.AllActive().Where(x => x.EmployeeTransferHistory.Any(h => h.ApprovalPersonID == employeeId)).Include(x=>x.Status).Include(x => x.Employee)
             .Include(x => x.FromOrganization).Include(x => x.ToOrganization)
             .Include(x => x.FromOrganizationBranch).Include(x => x.ToOrganizationBranch).Include(x => x.FromDepartment)
             .Include(x => x.ToDepartment).Include(x => x.FromDesignation).Include(x => x.ToDesignation)
             .OrderByDescending(x => x.EmployeeTransferID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                // 🔹 Step 4: Filter if not SuperAdmin
                //if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                //{
                //    query = query.Where(x => x.EmployeeID == employeeId);
                //}



                // Get all EmployeeOfficeInfo for filtering
                var officeInfoQuery = empoffi.AllActive().AsQueryable();

                if (organizationId.HasValue)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => x.OrganizationID == organizationId)
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(x => empIds.Contains(x.EmployeeID));
                }

                if (departmentIds?.Any() == true)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => departmentIds.Contains(x.DepartmentID ?? 0))
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(x => empIds.Contains(x.EmployeeID));
                }

                if (employeeIds?.Any() == true)
                {
                    query = query.Where(x => employeeIds.Contains((int)x.EmployeeID));
                }
                if (!string.IsNullOrEmpty(currentSortColumn))
                {
                    switch (currentSortColumn.ToLower())
                    {
                        case "employeename":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName)
                                : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                            break;

                        case "fromorganization":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.FromOrganization.OrganizationName)
                                : query.OrderByDescending(x => x.FromOrganization.OrganizationName);
                            break;

                        case "toorganization":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.ToOrganization.OrganizationName)
                                : query.OrderByDescending(x => x.ToOrganization.OrganizationName);
                            break;

                        case "frombranch":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.FromOrganizationBranch.OrganizationBranchName)
                                : query.OrderByDescending(x => x.FromOrganizationBranch.OrganizationBranchName);
                            break;

                        case "tobranch":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.ToOrganizationBranch.OrganizationBranchName)
                                : query.OrderByDescending(x => x.ToOrganizationBranch.OrganizationBranchName);
                            break;

                        case "transferdate":
                            query = currentSortOrder == "asc"
                                ? query.OrderBy(x => x.TransferDate)
                                : query.OrderByDescending(x => x.TransferDate);
                            break;

                        default:
                            query = query.OrderByDescending(x => x.EmployeeTransferID);
                            break;
                    }
                }

                var result = await PaginationService<EmployeeTransfer, EmployeeTransferTableListVM>.GetPaginatedData(



                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                   term => b =>
                   EF.Functions.Like(b.EmployeeTransferID.ToString(), $"%{term}%") ||
                   EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                   EF.Functions.Like(b.FromOrganization.OrganizationName, $"%{term}%") ||
                   EF.Functions.Like(b.ToOrganization.OrganizationName, $"%{term}%") ||
                   EF.Functions.Like(b.FromOrganizationBranch.OrganizationBranchName, $"%{term}%") ||
                   EF.Functions.Like(b.ToOrganizationBranch.OrganizationBranchName, $"%{term}%"),
                   b => new EmployeeTransferTableListVM
                   {

                       EmployeeTransferID = b.EmployeeTransferID,
                       TransferDate = DateTimeHelpers.FormatDateTime(b.TransferDate),
                       FromOrganizationName = b.FromOrganization?.OrganizationName ?? "",
                       ToOrganizationName = b.ToOrganization?.OrganizationName ?? "",
                       FromOrganizationBranchName = b.FromOrganizationBranch?.OrganizationBranchName ?? "",
                       ToOrganizationBranchName = b.ToOrganizationBranch?.OrganizationBranchName ?? "",
                       FromDepartmentName = b.FromDepartment.DepartmentName,
                       ToDepartmentName = b.ToDepartment.DepartmentName,
                       FromDesignationName = b.FromDepartment.DepartmentName,
                       EmployeeName = b.Employee != null ? $"{b.Employee.FirstName ?? ""} {b.Employee.LastName ?? ""}".Trim() : "",
                       EmployeeImage = b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                       EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),
                       StatusName=b.Status !=null ? b.Status.StatusName  : ""   
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
        public async Task<List<CommonSelectVM>> GetAllEmployee(string userId)
        {
            // Step 1: Get employeeId from the user
            //var employeeId = await appDb.Users
            //    .Where(u => u.Id == userId)
            //    .Select(e => e.EmployeeId)
            //    .FirstOrDefaultAsync();

            //// Step 2: Get the role name
            //var roleName = await (from user in appDb.Users
            //                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
            //                      join role in appDb.Roles on userRole.RoleId equals role.Id
            //                      where user.Id == userId
            //                      select role.Name)
            //                     .FirstOrDefaultAsync();

            //// Step 3: If not Admin, return only that employee
            //if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            //{
            //    var data = await employee.AllActive()
            //        .Where(x => x.EmployeeID == employeeId)
            //        .Select(x => new CommonSelectVM
            //        {
            //            Id = x.EmployeeID,
            //            Name = $"{x.FirstName} {x.LastName}"
            //        }).ToListAsync();

            //    return data;
            //}

            // Step 4: If Admin, return all employees
            var allData = await employee.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName}"
                }).ToListAsync();

            return allData;
        }

        #endregion
        #region Get EmployeeTransfer

        public async Task<CommonReturnViewModel> GetEmployeeTransferByIdAsync(int employeeTransferID)
        {
            try
            {
                if (employeeTransferID <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid transfer ID"
                    };
                }

                var existenceEntity = await repositoryEmployeeTransfer.GetByIdAsync(employeeTransferID);

                if (existenceEntity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Transfer record not found"
                    };
                }

                var viewModel = new EmployeeTransferApproveOrDecEditVM
                {
                    EmployeeTransferID = existenceEntity.EmployeeTransferID,
                    EmployeeIDEdit = existenceEntity.EmployeeID,
                    FromOrganizationIDEdit = existenceEntity.FromOrganizationID,
                    FromOrganizationBranchIDEdit = existenceEntity.FromOrganizationBranchID,
                    ToOrganizationIDEdit = existenceEntity.ToOrganizationID,
                    ToOrganizationBranchIDEdit = existenceEntity.ToOrganizationBranchID,
                    TransferDateEdit = existenceEntity.TransferDate,
                    TransferNoteEdit = existenceEntity.TransferNote,
                    FromDepartmentIDEdit = existenceEntity.FromDepartmentID,
                    ToDepartmentIDEdit = existenceEntity.ToDepartmentID,
                    ToDesignationIDEdit = existenceEntity.ToDesignationID,
                    FromDesignationIDEdit = existenceEntity.FromDesignationID,
                    TransferTypeEdit = existenceEntity.TransferType,
                };

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = viewModel
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred while fetching the transfer data: {ex.Message}"
                };
            }
        }




        #endregion

        #region Update Data
        private async Task<int?> GetIdByNameAsync(string name)
        {
            var data = await statusRepository.AllActive().Where(x => EF.Functions.Like(x.StatusName.ToLower(), name.ToLower())).Select(x => (int?)x.StatusID).FirstOrDefaultAsync();

            return data;
        }

        public async Task<CommonReturnViewModel> UpdateEmployeeTransferAsync(EmployeeTransferApproveOrDecEditVM entityVM)
        {
            await repositoryEmployeeTransfer.BeginTransactionAsync();

            if (entityVM == null || entityVM.EmployeeIDEdit == 0)
            {
                return new CommonReturnViewModel { Success = false, Message = "Invalid data." };
            }

            try
            {

                //
                var offf = await empoffi.AllActive()
    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
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
                        x.ApprovalType.ApprovalTypeName == "Transfer Approval").FirstOrDefaultAsync();

                if (approvalSettings == null) return new CommonReturnViewModel { Success = false, Message = "You have no permision transfer approval" };

                //
                // Step 1: Find existing transfer record
                var existingEntity = await repositoryEmployeeTransfer.AllActive()
.FirstOrDefaultAsync(x => x.EmployeeID == entityVM.EmployeeIDEdit );

                if (existingEntity == null)
                {
                    await repositoryEmployeeTransfer.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Transfer record not found"
                    };
                }

                // Optional: Check for duplicate if necessary
                var isDuplicate = await repositoryEmployeeTransfer.AllActive()
                    .AnyAsync(x =>
                        x.EmployeeID == entityVM.EmployeeIDEdit &&
                        x.ToOrganizationID == entityVM.ToOrganizationIDEdit &&
                        x.ToOrganizationBranchID == entityVM.ToOrganizationBranchIDEdit &&
                        x.TransferDate == entityVM.TransferDateEdit &&
                        x.EmployeeTransferID != existingEntity.EmployeeTransferID 
                    );

                if (isDuplicate)
                {
                    await repositoryEmployeeTransfer.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Duplicate transfer record already exists."
                    };
                }
                int? statusApproved = await GetIdByNameAsync("APPROVED");
                int? statusDecline = await GetIdByNameAsync("DECLINED");

                int? statusId = entityVM.Approved ? statusApproved : statusDecline;
                if (statusId == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Approval or Decline must be selected."
                    };
                }
                bool isFirstApprover = approvalSettings !=null && approvalSettings?.FirstApprovalID == entityVM.UpdatedBy;
                bool isSecondApprover = approvalSettings != null && approvalSettings?.SecondApprovalID == entityVM.UpdatedBy;
                bool isThirdApprover = approvalSettings != null && approvalSettings?.ThirdApprovalID == entityVM.UpdatedBy;
                bool allowSelfApprover = approvalSettings != null && approvalSettings.SelfExceptionApprovalID == entityVM.UpdatedBy;
                if (!isFirstApprover && !isSecondApprover && !isThirdApprover && !allowSelfApprover)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "You are not authorized to approve this leave request."
                    };
                }

                int approvalStep = isFirstApprover ? 1 : isSecondApprover ? 2 : isThirdApprover ? 3 : allowSelfApprover ? 4 : 0;
               
                bool isFinalApproval = false;

                if (isFirstApprover && approvalSettings.IsEnableSecondApproval && entityVM.Approved)
                {

                    existingEntity.StatusID = statusId;
                    existingEntity.ApprovalPersonID = approvalSettings.SecondApprovalID;
                }
                else if (isSecondApprover && approvalSettings.IsEnableThirdApproval && entityVM.Approved)
                {
                    existingEntity.StatusID = statusId;
                    existingEntity.ApprovalPersonID = approvalSettings.ThirdApprovalID;
                    
                }
                else if (isThirdApprover)
                {
                    existingEntity.StatusID = statusId;
                    existingEntity.ApprovalPersonID = approvalSettings.ThirdApprovalID;
                    isFinalApproval = true;
                }
                else if (allowSelfApprover && approvalSettings.AllowSelfApproval.HasValue && !approvalSettings.AllowSelfApproval.Value && entityVM.Approved)
                {
                    existingEntity.StatusID = statusId;
                    existingEntity.ApprovalPersonID = approvalSettings.SelfExceptionApprovalID;
                    isFinalApproval = true;
                
                }
                else
                {
                    existingEntity.StatusID = statusId;
                    existingEntity.ApprovalPersonID =approvalSettings.FirstApprovalID;
                    isFinalApproval = true;
                }
                existingEntity.TransferNote = entityVM.TransferNoteEdit;
                existingEntity.IsFinalApproved = isFinalApproval;
                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = entityVM.UpdatedBy;
                await repositoryEmployeeTransfer.UpdateAsync(existingEntity);
                var empTransferHistory = new EmployeeTransferHistory
                {

                    FromOrganizationID = entityVM.FromOrganizationIDEdit,
                    ToOrganizationID = entityVM.ToOrganizationIDEdit,
                    FromOrganizationBranchID = entityVM.FromOrganizationBranchIDEdit,
                    ToOrganizationBranchID = entityVM.ToOrganizationBranchIDEdit,
                    FromDesignationID = entityVM.FromDesignationIDEdit,
                    ToDesignationID = entityVM.ToDesignationIDEdit,
                    FromDepartmentID = entityVM.FromDepartmentIDEdit,
                    ToDepartmentID = entityVM.ToDepartmentIDEdit,
                    TransferType = "Transfer Approval",
                    ApprovalPersonNote = entityVM.TransferNoteEdit,
                    ApprovalStep = approvalStep,
                    StatusID= statusId,
                    EmployeeTransferID = existingEntity.EmployeeTransferID,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LIP,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    ApprovalPersonID = entityVM.CreatedBy,
                };
                await transferhistoryRepository.AddAsync(empTransferHistory);

                // Step 3: Update employee official info
                if(isFinalApproval && isFinalApproval)
                {
                    var getByIdEmpoffi = await empoffi.AllActive().FirstOrDefaultAsync(x => x.EmployeeID == entityVM.EmployeeIDEdit);

                    if (getByIdEmpoffi == null)
                    {
                        await repositoryEmployeeTransfer.RollbackTransactionAsync();
                        return new CommonReturnViewModel
                        {
                            Success = false,
                            Message = "Employee not found in official info table"
                        };
                    }

                    if (entityVM.TransferTypeEdit == "Organization")
                    {
                        getByIdEmpoffi.OrganizationID = entityVM.ToOrganizationIDEdit;
                        getByIdEmpoffi.OrganizationBranchID = entityVM.ToOrganizationBranchIDEdit;
                    }
                    else if (entityVM.TransferTypeEdit == "Branch")
                    {
                        getByIdEmpoffi.OrganizationBranchID = entityVM.ToOrganizationBranchIDEdit;
                    }
                    getByIdEmpoffi.DesignationID = entityVM.ToDesignationIDEdit;
                    getByIdEmpoffi.DepartmentID = entityVM.ToDepartmentIDEdit;
                    await empoffi.UpdateAsync(getByIdEmpoffi);  

                }

                //
                // Step 4: Commit transaction
                await repositoryEmployeeTransfer.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated Successfully"
                };
            }
            catch (Exception ex)
            {
                await repositoryEmployeeTransfer.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred while updating: {ex.Message}"
                };
            }
        }

        #endregion
    }
}
