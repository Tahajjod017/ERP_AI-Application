using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.Employee.EmpTransfer;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<OrganizationBranches> organizationBranch;
        private readonly IGenericRepository<Alerts> alertsRepository;
        private readonly IGenericRepository<EmployeeTransferHistory> transferhistoryRepository;
        private readonly IGenericRepository<AlertForEmployee> alertForEmployeeRepository;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        private readonly IGenericRepository<Statuses> statusRepository;
        public EmployeeTransferService(IGenericRepository<EmployeeTransfer> genericRepository, IGenericRepository<EmployeeTransfer> repositoryEmployeeTransfer, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<OrganizationBranches> organizationBranch, IGenericRepository<Alerts> alertsRepository, IGenericRepository<EmployeeTransferHistory> transferhistoryRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IGenericRepository<Statuses> statusRepository) : base(genericRepository)
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

                var query = repositoryEmployeeTransfer.AllActive().Include(x=>x.Status).Include(x => x.Employee)
             .Include(x => x.FromOrganization).Include(x => x.ToOrganization)
             .Include(x => x.FromOrganizationBranch).Include(x => x.ToOrganizationBranch).Include(x=>x.FromDepartment)
             .Include(x=>x.ToDepartment).Include(x=>x.FromDesignation).Include(x => x.ToDesignation)
             .OrderByDescending(x => x.EmployeeTransferID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                // 🔹 Step 4: Filter if not SuperAdmin
                if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.EmployeeID == employeeId);
                }


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
                       FromDesignationName=b.FromDepartment.DepartmentName, 
                       StatusName = b.Status !=null ? b.Status.StatusName : "",
                       EmployeeName = b.Employee != null ? $"{b.Employee.FirstName ?? ""} {b.Employee.LastName ?? ""}".Trim(): "",
                       EmployeeImage = b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName)? url + b.Employee.EmployeeImageFileName : "",
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


        #endregion

        #region IsExistAnyRole
        public async Task<CommonReturnViewModel> CheckIfEmployeeHasApprovalRoleAsync(EmployeeTransferAddVM entityVM, int? approvalPersonId)
        {
            
            // Check if employee is in any ApprovalSettings role
            var hasRoleInApprovalSettings = await approvalSettingsRepository.AllActive().AnyAsync(x =>
                x.FirstApprovalID == entityVM.EmployeeID ||
                x.SecondApprovalID == entityVM.EmployeeID ||
                x.ThirdApprovalID == entityVM.EmployeeID ||
                x.SelfExceptionApprovalID==entityVM.EmployeeID 
            );

            // Check if employee is set as any supervisor in EmployeeOfficeInfo
            var hasRoleInOfficeInfo = await empoffi.AllActive().AnyAsync(x =>
                x.SeniorSupervisorId == entityVM.EmployeeID ||
                x.ImmediateSupervisorId == entityVM.EmployeeID ||
                x.HeadOfDepartmentId == entityVM.EmployeeID
            // DO NOT check EmploymentStatus here unless it's a navigation property loaded
            );

            if (hasRoleInApprovalSettings || hasRoleInOfficeInfo)
            {
                // Create alert
                var alert = new Alerts
                {
                    AlertTitle = "Employee Transfer",
                    AlertNote = "This employee is currently assigned to an approval or supervisory role.",
                    LMAC = entityVM.LMAC,
                    LIP = entityVM.LIP,
                    CreatedBy = entityVM.CreatedBy,
                    CreatedAt = DateTime.Now,
                };

                await alertsRepository.AddAsync(alert);
                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = approvalPersonId,  // for alert Employee
                    IsChecked = false,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                };
                await alertForEmployeeRepository.AddAsync(empAlert);
             
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "This employee is currently assigned to an approval or supervisory role. An alert has been created."
                };
            }

            return new CommonReturnViewModel
            {
                Success = true
            };
        }

        #endregion


        #region Save Data
        // Approver persons according to Designation
        private async Task<int?> ResolveApprovalAsync(int? approvalId, bool isDesignation, dynamic offf)
        {
            if (!approvalId.HasValue) return null;

            if (isDesignation)
            {
                var code = await approvaldesignation.AllActive()
                    .Where(x => x.ApprovalDesignationID == approvalId)
                    .Select(x => x.Code).FirstOrDefaultAsync();

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
        public async Task<CommonReturnViewModel> SaveEmployeeTansferAsync(EmployeeTransferAddVM entityVM)
        {
            await repositoryEmployeeTransfer.BeginTransactionAsync();
            // Check Rul
            
            if (entityVM == null || entityVM.EmployeeID == 0)
            {
                return new CommonReturnViewModel { Success = false, Message = "Invalid data." };
            }

            var isDuplicate = await repositoryEmployeeTransfer.AllActive()
                .AnyAsync(x =>
                    x.EmployeeID == entityVM.EmployeeID &&
                    x.ToOrganizationID == entityVM.ToOrganizationID &&
                    x.ToOrganizationBranchID == entityVM.ToOrganizationBranchID &&
                    x.TransferDate == entityVM.TransferDate
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

            try
            {
                //
                var offf = await empoffi.AllActive().Where(x => x.EmployeeID == entityVM.EmployeeID)
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

                if (approvalSettings == null) return new CommonReturnViewModel { Success = false, Message = "No active Transfer  Approval settings found." };

                var approvalFlow = new List<(int? id, bool isDesignation)>
                {
                    (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
                    (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
                    (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
                };
                if (approvalSettings == null) return new CommonReturnViewModel { Success = false, Message = "No active Transfer Approval  settings found." };
                int? approvalPersonId = null;
                // Get which approver the employee is
                bool isFirstApprover = approvalSettings.FirstApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpFirstApprovalID;
                bool isSecondApprover = approvalSettings.SecondApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpSecondApprovalID && approvalSettings.IsEnableSecondApproval;
                bool isThirdApprover = approvalSettings.ThirdApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpThirdApprovalID && approvalSettings.IsEnableThirdApproval;

                if (isFirstApprover)
                {
                    // Go to second approver if available
                    if (approvalSettings.IsEnableSecondApproval && approvalSettings.SecondApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.SecondApprovalID.Value;
                    }
                    else if (approvalSettings.IsEnableThirdApproval && approvalSettings.ThirdApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.ThirdApprovalID.Value;
                    }
                    else if (approvalSettings.AllowSelfApproval == false && approvalSettings.SelfExceptionApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.SelfExceptionApprovalID.Value;
                    }
                }
                else if (isSecondApprover)
                {
                    // Go to third approver
                    if (approvalSettings.IsEnableThirdApproval && approvalSettings.ThirdApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.ThirdApprovalID.Value;
                    }
                    else if (approvalSettings.AllowSelfApproval == false && approvalSettings.SelfExceptionApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.SelfExceptionApprovalID.Value;
                    }
                }
                else if (isThirdApprover)
                {
                    if (approvalSettings.AllowSelfApproval == false && approvalSettings.SelfExceptionApprovalID.HasValue)
                    {
                        approvalPersonId = approvalSettings.SelfExceptionApprovalID.Value;
                    }
                }
                else
                {
                    // General Employee - start with First Approver
                    approvalPersonId = approvalSettings.FirstApprovalID ?? 0;
                }

                // Handle if no valid approver found
                if (approvalPersonId == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No valid approver found. Transfer request cannot be self-approved."
                    };
                }
                // Step 3: Normal approval flow (fallback logic)
                foreach (var (id, isDesignation) in approvalFlow)
                {
                    var resolvedId = await ResolveApprovalAsync(id, isDesignation, offf);

                    // Skip if resolved is the applicant
                    if (resolvedId == entityVM.CreatedBy)
                    {
                        continue;
                    }

                }
                // Final step: Process leave
                if (approvalPersonId == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No valid approver found." };
                }
              
                // Step 1: Save transfer record
                int employeeTransferID = 0;
                var entity = new EmployeeTransfer
                {
                    EmployeeID = entityVM.EmployeeID,
                    FromOrganizationID = entityVM.FromOrganizationID,
                    ToOrganizationID = entityVM.ToOrganizationID,
                    FromOrganizationBranchID = entityVM.FromOrganizationBranchID,
                    ToOrganizationBranchID = entityVM.ToOrganizationBranchID,
                    TransferNote = entityVM.TransferNote,
                    TransferDate = entityVM.TransferDate,
                    FromDepartmentID= entityVM.FromDepartmentID,
                    FromDesignationID= entityVM.FromDesignationID,
                    ToDepartmentID= entityVM.ToDepartmentID,
                    ToDesignationID= entityVM.ToDesignationID,
                    TransferType = entityVM.TransferType ?? "",
                    ApprovalPersonID=approvalPersonId,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy
                };

                await repositoryEmployeeTransfer.AddAsync(entity);
                employeeTransferID = entity.EmployeeTransferID;
                // Step 2: Update EmployeeOfficeInfo
                //var getByIdEmpoffi = await empoffi.AllActive() .FirstOrDefaultAsync(x => x.EmployeeID == entityVM.EmployeeID);

                //if (getByIdEmpoffi == null)
                //{
                //    await repositoryEmployeeTransfer.RollbackTransactionAsync();
                //    return new CommonReturnViewModel
                //    {
                //        Success = false,
                //        Message = "Employee not found in official info table"
                //    };
                //}
                
                //if (entityVM.TransferType == "Organization")
                //{
                //    getByIdEmpoffi.OrganizationID = entityVM.ToOrganizationID;
                //    getByIdEmpoffi.OrganizationBranchID = entityVM.ToOrganizationBranchID;
                //}
                //else if (entityVM.TransferType == "Branch")
                //{
                //    getByIdEmpoffi.OrganizationBranchID = entityVM.ToOrganizationBranchID;
                //}
                //getByIdEmpoffi.DesignationID = entityVM.ToDesignationID;
                //getByIdEmpoffi.DepartmentID = entityVM.ToDepartmentID;
                //await empoffi.UpdateAsync(getByIdEmpoffi);  

                //var empTransferHistory = new EmployeeTransferHistory
                //{
                    
                //    FromOrganizationID = entityVM.FromOrganizationID,
                //    ToOrganizationID = entityVM.ToOrganizationID,
                //    FromOrganizationBranchID = entityVM.FromOrganizationBranchID,
                //    ToOrganizationBranchID = entityVM.ToOrganizationBranchID,
                //    FromDesignationID = entityVM.FromDesignationID,
                //    ToDesignationID = entityVM.ToDesignationID,
                //    FromDepartmentID = entityVM.FromDepartmentID,
                //    ToDepartmentID = entityVM.ToDepartmentID,
                //    TransferType = "Transfer Approval",
                //    ApprovalPersonNote =entityVM.TransferNote,
                //    ApprovalStep=0,
                //    //StatusID=4,
                //    EmployeeTransferID= employeeTransferID, 
                //    LIP=entityVM.LIP,
                //    LMAC=entityVM.LIP,
                //    CreatedAt=DateTime.Now,
                //    CreatedBy=entityVM.CreatedBy,
                //    ApprovalPersonID=approvalPersonId,
                //};
                //await transferhistoryRepository.AddAsync(empTransferHistory);
                await CheckIfEmployeeHasApprovalRoleAsync(entityVM, approvalPersonId);   // Role Checked
                await repositoryEmployeeTransfer.CommitTransactionAsync();
                return new CommonReturnViewModel { Success = true, Message = "Saved Successfully" };
            }
            catch (Exception ex)
            {
                // Step 4: Rollback on error
                await repositoryEmployeeTransfer.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred while saving: {ex.Message}"
                };
            }
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

                var viewModel = new EmployeeTransferEditVM
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
                    FromDesignationIDEdit= existenceEntity.FromDesignationID,
                    TransferTypeEdit= existenceEntity.TransferType,
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

        public async Task<CommonReturnViewModel> UpdateEmployeeTransferAsync(EmployeeTransferEditVM entityVM)
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

                //if (approvalSettings != null) return new CommonReturnViewModel { Success = false, Message = "This employee is currently part of an active Leave Request Approval configuration. Please remove the approval role before transferring." };

                //
                // Step 1: Find existing transfer record
                var existingEntity = await repositoryEmployeeTransfer.AllActive()
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeID == entityVM.EmployeeIDEdit
                       
                    );

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
                        x.EmployeeTransferID != existingEntity.EmployeeTransferID // exclude current
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

                // Step 2: Update transfer fields
                existingEntity.FromOrganizationID = entityVM.FromOrganizationIDEdit;
                existingEntity.FromOrganizationBranchID = entityVM.FromOrganizationBranchIDEdit;
                existingEntity.ToOrganizationID = entityVM.ToOrganizationIDEdit;
                existingEntity.ToOrganizationBranchID = entityVM.ToOrganizationBranchIDEdit;
                existingEntity.FromDepartmentID = entityVM.FromDepartmentIDEdit;
                existingEntity.ToDepartmentID = entityVM.ToDepartmentIDEdit;
                existingEntity.FromDesignationID = entityVM.FromDesignationIDEdit;
                existingEntity.TransferNote = entityVM.TransferNoteEdit;
                existingEntity.TransferDate = entityVM.TransferDateEdit;
                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = entityVM.UpdatedBy;
                await repositoryEmployeeTransfer.UpdateAsync(existingEntity);
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

        #region Delete Leave Request
        public async Task<CommonReturnViewModel> SoftDeleteEmpTransfer(DeleteRequestVM deleteRequestVM)
        {
            await repositoryEmployeeTransfer.BeginTransactionAsync();
            try
            {
                var data = await repositoryEmployeeTransfer.FindAsync(x => deleteRequestVM.Ids.Contains(x.EmployeeTransferID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No data found to delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<AddNewLeaveSave>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.EmployeeTransferID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = deleteRequestVM.LIP;
                    item.LMAC = deleteRequestVM.LMAC;
                    item.DeletedBy = deleteRequestVM.DeletedBy ?? null;
                }

                await repositoryEmployeeTransfer.UpdateRangeAsync(data);
               // await userInfoService.ActionLogDeleteAsync("Leave Settigs", ActionName.DataDeleted, null, beforeEntity, targetIds, deleteRequestVM);
                await repositoryEmployeeTransfer.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = $"Deleted Successfully."

                };
            }
            catch (Exception ex)
            {
                await repositoryEmployeeTransfer.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion

        #region  Get Organizationwith Branch accordig to Employee


        public async Task<CommonReturnViewModel> GetEmpOrganizationBranchId(int employeeID)
        {
            if (employeeID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid Employee ID"
                };
            }

            var data = await empoffi.AllActive()
                .Where(x => x.EmployeeID == employeeID)
                .Select(x => new EmpOrganizationOrganizationBranchGetByIdVM
                {
                    FromOrganizationID = x.OrganizationID,
                    FromOrganizationBranchID = x.OrganizationBranchID,
                    FromDepartmentID= x.DepartmentID,
                    FromDesignationID= x.DesignationID,
                }).FirstOrDefaultAsync();

            if (data == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee not found"
                };
            }

            return new CommonReturnViewModel
            {
                Success = true,
                Data = data
            };
        }

        #endregion


        #region  GetBranch  acoording to organization 

        public  async Task<CommonReturnViewModel> GetEmpBranchId(int toOrganizationID)
        {
            if (toOrganizationID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid ToOrganization ID"
                };
            }

            var branch = await organizationBranch.AllActive()
                .Where(x => x.OrganizationID == toOrganizationID)
                .Select(x => new CommonSelectVM
                {
                    Id = x.OrganizationBranchID,
                    Name = x.OrganizationBranchName,
                }).FirstOrDefaultAsync();

            if (branch == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Branch not found"
                };
            }

            return new CommonReturnViewModel
            {
                Success = true,
                Data = branch
            };
        }



        #endregion

        #region Person Leave Step  

        public async Task<List<PersonTransferStepVM>> GetByPersonTransferStepVM(int employeeTransferID)
        {
            try
            {
                var result = await (
                    from lb in transferhistoryRepository.AllActive()
                        .Where(x => x.EmployeeTransferID == employeeTransferID)
                        .AsNoTracking()
                    join statusName in statusRepository.AllActive()
                        .Select(x => new { x.StatusID, x.StatusName })
                        on lb.StatusID equals statusName.StatusID
                    join e in employee.AllActive()
                        .Select(x => new { x.EmployeeID, x.FirstName, x.LastName })
                        on lb.ApprovalPersonID equals e.EmployeeID
                    
                    select new PersonTransferStepVM
                    {
                        ApprovarNote = lb.ApprovalPersonNote ?? string.Empty,
                        ApproverStep = lb.ApprovalStep ?? 0,
                        ApprovarPerson = e.FirstName + " " + e.LastName ?? string.Empty,
                        StatusName = statusName.StatusName ?? string.Empty,
                        ApprovedOrDeclineDate = DateTimeHelpers.FormatDateTime(lb.CreatedAt),

                    }).OrderBy(X => X.ApproverStep).ToListAsync();

                return result ?? new List<PersonTransferStepVM>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
