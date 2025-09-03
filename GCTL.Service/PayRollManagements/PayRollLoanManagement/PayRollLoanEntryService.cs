using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public class PayRollLoanEntryService:AppService<Loan>, IPayRollLoanEntryService
    {
        private readonly IGenericRepository<Loan> loanRepository;
        private IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        public readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<LoanBaseApprovalHistory> loanBaseHistory;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        private readonly IGenericRepository<Statuses> status;
        private readonly IUserInfoService _userInfoService;
        public PayRollLoanEntryService(IGenericRepository<Loan> loanRepository, IGenericRepository<LoanInstallmentPeriods> loanInstallment, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<LoanBaseApprovalHistory> loanBaseHistory, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IGenericRepository<Statuses> status, IUserInfoService userInfoService) : base(loanRepository)
        {
            this.loanRepository = loanRepository;
            this.loanInstallment = loanInstallment;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.employee = employee;
            this.loanBaseHistory = loanBaseHistory;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.approvaldesignation = approvaldesignation;
            this.status = status;
            _userInfoService = userInfoService;
        }
        #region Save Data

        public async Task<CommonReturnViewModel> SaveAsync(LoanSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();
            if (!entityVM.LoanAmount.HasValue)
            {
                result.Success = false;
                result.Message = "Loan amount is required.";
                return result;
            }
            if (entityVM.LoanAmount <= 0 || entityVM.LoanAmount > 1000000)
            {
                result.Success = false;
                result.Message = "Loan amount must be greater than 0 and not exceed 10,00,000.";
                return result;
            }
            int? loanInstallmentPeriodID = await loanInstallment.AllActive().Where(x => x.PeriodValue == entityVM.LoanInstallmentPeriodID).Select(x => x.LoanInstallmentPeriodID).FirstOrDefaultAsync();
            try
            {
                //
              entityVM.EmployeeID = entityVM.EmployeeIDs.FirstOrDefault();
                var offf = await empoffi.AllActive()
                    .Where(x => x.EmployeeID == entityVM.EmployeeID)
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

                // Get approval settings
                var approvalSettings = await approvalSettingsRepository.AllActive()
                    .Include(x => x.ApprovalType)
                    .Where(x =>
                        x.ApprovalType.ApprovalTypeName == "Loan Approval")
                    .FirstOrDefaultAsync();

                if (approvalSettings == null)
                    return new CommonReturnViewModel { Success = false, Message = "No active Loan Approval settings found." };

                // Build approval flow
                var approvalFlow = new List<(int? id, bool isDesignation)>
                    {
                        (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
                        (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
                        (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
                    };

                int? approvalPersonId = null;
                // Determine if the current employee is already an approver
                bool isFirstApprover = approvalSettings.FirstApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpFirstApprovalID;
                bool isSecondApprover = approvalSettings.SecondApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpSecondApprovalID && approvalSettings.IsEnableSecondApproval;
                bool isThirdApprover = approvalSettings.ThirdApprovalID == entityVM.EmployeeID && !approvalSettings.IsDesignationOrEmpThirdApprovalID && approvalSettings.IsEnableThirdApproval;

                // Determine next approver
                if (isFirstApprover)
                {
                    approvalPersonId = await ResolveApprovalAsync(approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID, offf)
                        ?? await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf)
                        ?? (approvalSettings.AllowSelfApproval == false
                            ? await ResolveApprovalAsync(approvalSettings.SelfExceptionApprovalID, false, offf)
                            : null);
                }
                else if (isSecondApprover)
                {
                    approvalPersonId = await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf)
                        ?? (approvalSettings.AllowSelfApproval == false
                            ? await ResolveApprovalAsync(approvalSettings.SelfExceptionApprovalID, false, offf)
                            : null);
                }
                else if (isThirdApprover)
                {
                    approvalPersonId = (approvalSettings.AllowSelfApproval == false && approvalSettings.SelfExceptionApprovalID.HasValue)
                        ? await ResolveApprovalAsync(approvalSettings.SelfExceptionApprovalID, false, offf)
                        : null;
                }
                else
                {
                    // General Employee - start from top of the flow
                    foreach (var (id, isDesignation) in approvalFlow)
                    {
                        var resolvedId = await ResolveApprovalAsync(id, isDesignation, offf);

                        if (resolvedId.HasValue && resolvedId != entityVM.CreatedBy)
                        {
                            approvalPersonId = resolvedId;
                            break;
                        }
                    }
                }

                // Final fallback
                if (approvalPersonId == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "No valid approver found cannot be self-approved."
                    };
                }
                //
                await loanRepository.BeginTransactionAsync();
                Loan loan = new Loan
                {
                    EmployeeID = entityVM.EmployeeIDs.FirstOrDefault(),
                    LoanInstallmentPeriodID = loanInstallmentPeriodID,
                    LoanAmount = entityVM.LoanAmount.Value,
                    ApprovalPersonID= approvalPersonId,
                    IssueDate = entityVM.IssueDate,
                    StartDate = entityVM.StartDate,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };

                await loanRepository.AddAsync(loan);
                await _userInfoService.ActionLogAsync("Loan Entry", ActionName.DataAdd, null, loan, loan.LoanID, entityVM);
                await loanRepository.CommitTransactionAsync();
                result.Success = true;
                result.Message = "Loan Saved Successfully.";
                result.Data = loan; 
            }
            catch (DbUpdateException ex)
            {
                await loanRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "Failed to save loan.";
                result.Errors.Add(ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                await loanRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An unexpected error occurred.";
                result.Errors.Add(ex.Message);
            }
            return result;
        }
        #endregion
        private async Task<int?> GetIdByNameAsync(string name)
        {
            var data = await status.AllActive().Where(x => EF.Functions.Like(x.StatusName.ToLower(), name.ToLower())).Select(x => (int?)x.StatusID).FirstOrDefaultAsync();

            return data;
        }
        private async Task<int?> ResolveApprovalAsync(int? approvalId, bool isDesignation, dynamic offf)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }


        }
        #region

        #endregion
        public async Task<CommonReturnViewModel> UpdateFromAppDecAsync(PayRollLoanViewDeclineApprovedVM entityVM)
        {
            if (entityVM == null || entityVM.LoanID == 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid loan data."
                };
            }

            try
            {
                var offf = await empoffi.AllActive()
                   .Where(x => x.EmployeeID == entityVM.EmployeeIDs)
                   .Select(x => new { x.EmployeeID, x.OrganizationID, x.OrganizationBranchID, x.DepartmentID, x.DesignationID, x.ImmediateSupervisorId, x.SeniorSupervisorId, x.HeadOfDepartmentId }).FirstOrDefaultAsync();

                if (offf == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee office info not found."
                    };
                }
                // Get approval settings
                var approvalSettings = await approvalSettingsRepository.AllActive().Include(x => x.ApprovalType).FirstOrDefaultAsync(x =>
                        (x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID) && x.ApprovalType.ApprovalTypeName == "Loan Approval");
                if (approvalSettings == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Your Company Does not Exists in Approver Settings."
                    };
                }
                var approvalFlow = new List<(int? id, bool isDesignation)>
                    {
                        (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
                        (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
                        (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
                    };

                bool isFinalApproval = false;
                bool isFirstApprover = false;
                bool isSecondApprover = false;
                bool isThirdApprover = false;
                bool allowSelfApprover = false;
                int? approvalPersonId = null;
                if (!approvalSettings.IsDesignationOrEmpFirstApprovalID || !approvalSettings.IsDesignationOrEmpSecondApprovalID || !approvalSettings.IsDesignationOrEmpThirdApprovalID)
                {
                    isFirstApprover = approvalSettings != null && approvalSettings?.FirstApprovalID == entityVM.UpdatedBy;
                    isSecondApprover = approvalSettings != null && approvalSettings?.SecondApprovalID == entityVM.UpdatedBy;
                    isThirdApprover = approvalSettings != null && approvalSettings?.ThirdApprovalID == entityVM.UpdatedBy;
                    allowSelfApprover = approvalSettings != null && approvalSettings.SelfExceptionApprovalID == entityVM.UpdatedBy;
                }
                else if (approvalSettings.IsDesignationOrEmpFirstApprovalID || approvalSettings.IsDesignationOrEmpSecondApprovalID || approvalSettings.IsDesignationOrEmpThirdApprovalID)
                {

                    int? resolvedFirst = await ResolveApprovalAsync(approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID, offf);
                    int? resolvedSecond = await ResolveApprovalAsync(approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID, offf);
                    int? resolvedThird = await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf);
                    isFirstApprover = resolvedFirst == entityVM.UpdatedBy;
                    isSecondApprover = resolvedSecond == entityVM.UpdatedBy;
                    isThirdApprover = resolvedThird == entityVM.UpdatedBy;
                    if (isFirstApprover)
                    {
                        approvalPersonId = resolvedSecond;
                    }
                    else if (isSecondApprover)
                    {
                        approvalPersonId = resolvedThird;
                    }
                    else
                    {
                        approvalPersonId = resolvedThird;
                        if (entityVM.Approved)
                        {
                            isFinalApproval = true;
                        }
                    }
                }
                int approvalStep = 0;
                if (approvalSettings != null && !approvalSettings.IsDesignationOrEmpFirstApprovalID &&
                    !approvalSettings.IsDesignationOrEmpSecondApprovalID && !approvalSettings.IsDesignationOrEmpThirdApprovalID)
                {
                    approvalStep = isFirstApprover ? 1 : isSecondApprover ? 2 : isThirdApprover ? 3 : allowSelfApprover ? 4 : 0;
                }
                else if (approvalSettings != null && (approvalSettings.IsDesignationOrEmpFirstApprovalID ||
                         approvalSettings.IsDesignationOrEmpSecondApprovalID ||
                         approvalSettings.IsDesignationOrEmpThirdApprovalID))
                {

                    approvalStep = isFirstApprover ? 1 : isSecondApprover ? 2 : isThirdApprover ? 3 : 0;

                }
                if (!isFirstApprover && !isSecondApprover && !isThirdApprover && !allowSelfApprover)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "You are not authorized to approve this leave request."
                    };
                }
                // Get status IDs
                int? leavStatusApproved = await GetIdByNameAsync("APPROVED");
                int? leavStatusDecline = await GetIdByNameAsync("DECLINED");
                int? statusId = entityVM.Approved ? leavStatusApproved : leavStatusDecline;
                if (!statusId.HasValue)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Approval or Decline must be selected."
                    };
                }
                // 🔹 Authorization chec

                if (isFirstApprover && approvalSettings.IsEnableSecondApproval && !approvalSettings.IsDesignationOrEmpFirstApprovalID)
                {

                    approvalPersonId = approvalSettings.SecondApprovalID;
                }
                else if (isSecondApprover && approvalSettings.IsEnableThirdApproval && !approvalSettings.IsDesignationOrEmpSecondApprovalID)
                {
                    approvalPersonId = approvalSettings.ThirdApprovalID;
                }
                else if (isThirdApprover && !approvalSettings.IsDesignationOrEmpThirdApprovalID)
                {
                    approvalPersonId = approvalSettings.ThirdApprovalID;
                    if (entityVM.Approved)
                    {
                        isFinalApproval = true;
                    }

                }
                else if (allowSelfApprover && approvalSettings.AllowSelfApproval.HasValue && !approvalSettings.AllowSelfApproval.Value)
                {
                    approvalPersonId = approvalSettings.SelfExceptionApprovalID;
                    if (entityVM.Approved)
                    {
                        isFinalApproval = true;
                    }

                }
                //
                // Fetch existing loan from repository
                await loanRepository.BeginTransactionAsync();
                var loan = await loanRepository.GetByIdAsync(entityVM.LoanID);
                var beforeEntity = JsonConvert.DeserializeObject<GradeVM>(JsonConvert.SerializeObject(loan, JsonSettings.IgnoreReferenceLoop));
                if (loan == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan not found."
                    };
                }
                // Update fields
                //loan.EmployeeID = entityVM.EmployeeIDs ;
                loan.LoanAmount = entityVM.LoanAmount;
                loan.LoanInstallmentPeriodID = entityVM.LoanInstallmentPeriodID;
                loan.IssueDate = entityVM.IssueDate;
                loan.StartDate = entityVM.StartDate;
                loan.ApprovalStage = approvalStep;
                loan.ApprovalPersonID = approvalPersonId; //
                loan.IsFinalApproved = isFinalApproval;
                loan.UpdatedAt = DateTime.Now;
                loan.UpdatedBy = entityVM.UpdatedBy;
                loan.StatusID = statusId;
                loan.LIP = entityVM.LIP;
                loan.LMAC = entityVM.LMAC;
                await loanRepository.UpdateAsync(loan);
                var afterEntity = JsonConvert.DeserializeObject<GradeVM>(JsonConvert.SerializeObject(loan, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Loan Approver", ActionName.DataUpdated, beforeEntity, afterEntity, loan.LoanID, entityVM);
                var loanBase = new LoanBaseApprovalHistory
                {

                    LoanID = entityVM.LoanID,
                    ApproveByID = entityVM.CreatedBy,
                    ApprovalStep = approvalStep,
                    ApproverNote = entityVM.ApproverNote,
                    StatusID = statusId,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                };

                await loanBaseHistory.AddAsync(loanBase);
                await _userInfoService.ActionLogAsync("Loan History", ActionName.DataAdd, null, loanBase, loanBase.BaseApprovalHistoryID, entityVM);
                await loanRepository.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Loan updated successfully."
                };
            }
            catch (DbUpdateException ex)
            {
                await loanRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"Database error: {ex.Message}"

                };
            }
            catch (Exception ex)
            {
                await loanRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        public async Task<CommonReturnViewModel> UpdateAsync(LoanUpdateVM entityVM)
        {
            var result = new CommonReturnViewModel();
            try
            {
                // Validate input
                if (entityVM.LoanID <= 0)
                {
                    result.Success = false;
                    result.Message = "Invalid loan ID.";
                    result.Errors.Add("LoanID must be greater than zero.");
                    return result;
                }
                await loanRepository.BeginTransactionAsync();
                // Fetch existing loan
                var loan = await loanRepository.GetByIdAsync(entityVM.LoanID);
                if (loan == null)
                {
                    await loanRepository.RollbackTransactionAsync();
                    result.Success = false;
                    result.Message = "Loan not found.";
                    result.Errors.Add($"No loan found with ID {entityVM.LoanID}.");
                    return result;
                }

                // Update loan properties (only if provided in entityVM)
                if (entityVM.EmployeeID.HasValue)
                    loan.EmployeeID = entityVM.EmployeeID.Value;
                if (entityVM.LoanAmount.HasValue)
                    loan.LoanAmount = entityVM.LoanAmount.Value;
                if (entityVM.LoanInstallmentPeriodID.HasValue)
                    loan.LoanInstallmentPeriodID = entityVM.LoanInstallmentPeriodID.Value;
                if (entityVM.IssueDate.HasValue)
                    loan.IssueDate = entityVM.IssueDate.Value;
                if (entityVM.StartDate.HasValue)
                    loan.StartDate = entityVM.StartDate.Value;

                // Optionally update other fields (e.g., from BaseViewModel)
                if (entityVM is { UpdatedBy: not null })
                    loan.UpdatedBy = entityVM.UpdatedBy;
                loan.UpdatedAt = DateTime.Now;

                // Update in repository
                await loanRepository.UpdateAsync(loan);
                await loanRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Loan updated successfully.";
                result.Data = loan; // Optionally return the updated loan
            }

            catch (DbUpdateException ex)
            {
                await loanRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "Failed to update loan.";
                result.Errors.Add(ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                await loanRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "";
            }
            return result;
        }
        #region Update Data


        #endregion
        #region get By data

        public Task<CommonReturnViewModel> GetByAsync(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<CommonReturnViewModel> GetByIdApprovedOrDecline(int id)
        {
            try
            {

                var data = await loanRepository.GetByIdAsync(id);
                if (data == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan not found."
                    };
                }
                var result = new GetDataByID
                {
                    LoanID = data.LoanID,
                    EmployeeIDs = data.EmployeeID,
                    LoanAmount = data.LoanAmount,
                    LoanInstallmentPeriodID = data.LoanInstallmentPeriodID,
                    IssueDate = data.IssueDate,
                    StartDate = data.StartDate,
                };

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"Error fetching loan: {ex.Message}"
                };
            }
        }



        #endregion

        #region  Delete 
        public Task<CommonReturnViewModel> DeleteAsync(DeleteRequestVM deleteRequestVM)
        {
            throw new NotImplementedException();
        }


        #region get all Datum LoanView
        public async Task<PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>> LoanEntryList(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null)
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
                var query = loanRepository.AllActive().Include(x=>x.Status).Include(x => x.Employee).Include(x => x.LoanInstallmentPeriod).OrderByDescending(x => x.LoanID).AsQueryable();
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

                //
                Expression<Func<Loan, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.EmployeeID,  // Employee Id
                    "empname" => x => x.Employee.FirstName + " " + x.Employee.LastName,  // Employee Name
                    "empdept" => x => empoffi.AllActive()
                                              .Where(e => e.EmployeeID == x.EmployeeID)
                                              .Select(m => m.Department != null ? m.Department.DepartmentName : "")
                                              .FirstOrDefault(),  // Department
                    "emploanamount" => x => x.LoanAmount ?? 0,  // Loan Amount
                    //"empearlypayamount" => x => x.EarlyPayment ?? 0,  // Early Payment
                    "tenuremonth" => x => x.LoanInstallmentPeriod != null ? x.LoanInstallmentPeriod.PeriodText : "",  // Tenure Month
                    "monthlyemi" => x => (x.LoanAmount.HasValue && x.LoanInstallmentPeriod.PeriodValue.HasValue == true)
                                        ? x.LoanAmount.Value / x.LoanInstallmentPeriod.PeriodValue.Value : 0,  // Monthly EMI
                    "outstandindbalance" => x => (x.LoanAmount ?? 0) - ((x.LoanInstallmentPeriod.PeriodValue.HasValue)
                                         ? (x.LoanAmount ?? 0) / x.LoanInstallmentPeriod.PeriodValue.Value * x.LoanInstallmentPeriod.PeriodValue.Value
                                         : 0),
                    _ => x => x.LoanID  //
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);


                // For approver Step
                //
                var approvalStepsMap = await loanBaseHistory.AllActive().GroupBy(x => x.LoanID).ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ApprovalStep ?? 0).ToList());
                var result = await PaginationService<Loan, LoanViewGetAllVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.LoanID.ToString(), $"%{term}%") ||
                     EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                     EF.Functions.Like(empoffi.AllActive()
                        .Where(e => e.EmployeeID == b.EmployeeID)
                        .Select(m => m.Department != null ? m.Department.DepartmentName : "")
                        .FirstOrDefault(), $"%{term}%") ||
                      EF.Functions.Like((b.LoanAmount ?? 0).ToString(), $"%{term}%") ||
                      //EF.Functions.Like((b.EarlyPayment ?? 0).ToString(), $"%{term}%") ||       
                      EF.Functions.Like(b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "", $"%{term}%") ||
                      EF.Functions.Like(((b.LoanAmount ?? 0) / (b.LoanInstallmentPeriod.PeriodValue ?? 1)).ToString(), $"%{term}%") ||
                      EF.Functions.Like(((b.LoanAmount ?? 0) - (b.LoanInstallmentPeriod.PeriodValue ?? 0)).ToString(), $"%{term}%"),
                    b => new LoanViewGetAllVM
                    {
                        LoanID = b.LoanID,
                        ApplicationDate= b.CreatedAt?.ToString("dd-MM-yyyy hh:mm tt") ?? "",
                        EmployeeID = b.EmployeeID,
                        LoanAmount = b.LoanAmount ?? 0,
                        StatusName = b.Status?.StatusName ?? "",
                        TenureMonth = b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "",
                        MonthlyEMI = (b.LoanAmount.HasValue && b.LoanInstallmentPeriod?.PeriodValue.HasValue == true)
                 ? b.LoanAmount.Value / b.LoanInstallmentPeriod.PeriodValue.Value : 0,
                        EmployeeName = b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : "",

                        EmployeeImage = (b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName)) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive()
                        .Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department != null ? m.Department.DepartmentName : "").FirstOrDefault(),
                        ApproverStep =b.ApprovalStage   //approvalStepsMap.ContainsKey(b.LoanID) ? approvalStepsMap[b.LoanID] : new List<int>()
                    });


                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>
                {
                    Data = new List<LoanViewGetAllVM>(),
                    TotalCount = 0
                };
            }
        }

        #endregion


        #region get all Datum LoanAbove
        public async Task<PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>> GetAllTableAboveAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null)
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
                var query = loanRepository.AllActive().Include(x => x.Employee).Include(x => x.LoanInstallmentPeriod).OrderByDescending(x => x.LoanID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }

                // 🔹 Step 4: Filter if not SuperAdmin
                if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.ApprovalPersonID == employeeId);
                }
                //
                //
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

                //
                Expression<Func<Loan, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.EmployeeID,  
                    "empname" => x => x.Employee.FirstName + " " + x.Employee.LastName, 
                    "empdept" => x => empoffi.AllActive()
                                              .Where(e => e.EmployeeID == x.EmployeeID)
                                              .Select(m => m.Department != null ? m.Department.DepartmentName : "")
                                              .FirstOrDefault(), 
                    "emploanamount" => x => x.LoanAmount ?? 0,  
                    //"empearlypayamount" => x => x.EarlyPayment ?? 0,  // Early Payment
                    "tenuremonth" => x => x.LoanInstallmentPeriod != null ? x.LoanInstallmentPeriod.PeriodText : "",  
                    "monthlyemi" => x => (x.LoanAmount.HasValue && x.LoanInstallmentPeriod.PeriodValue.HasValue == true)
                                        ? x.LoanAmount.Value / x.LoanInstallmentPeriod.PeriodValue.Value : 0,  
                    "outstandindbalance" => x => (x.LoanAmount ?? 0) - ((x.LoanInstallmentPeriod.PeriodValue.HasValue )
                                         ? (x.LoanAmount ?? 0) / x.LoanInstallmentPeriod.PeriodValue.Value * x.LoanInstallmentPeriod.PeriodValue.Value
                                         : 0),  
                    _ => x => x.LoanID  //
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);
                var result = await PaginationService<Loan, LoanViewGetAllVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.LoanID.ToString(), $"%{term}%") ||                    
                     EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||  
                     EF.Functions.Like(empoffi.AllActive()
                        .Where(e => e.EmployeeID == b.EmployeeID)
                        .Select(m => m.Department != null ? m.Department.DepartmentName : "")
                        .FirstOrDefault(), $"%{term}%") ||                    
                      EF.Functions.Like((b.LoanAmount ?? 0).ToString(), $"%{term}%") ||        
                      //EF.Functions.Like((b.EarlyPayment ?? 0).ToString(), $"%{term}%") ||       
                      EF.Functions.Like(b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "", $"%{term}%") ||  
                      EF.Functions.Like(((b.LoanAmount ?? 0) / (b.LoanInstallmentPeriod.PeriodValue ?? 1)).ToString(), $"%{term}%") || 
                      EF.Functions.Like(((b.LoanAmount ?? 0) - (b.LoanInstallmentPeriod.PeriodValue ?? 0)).ToString(), $"%{term}%")  ,  
                    b => new LoanViewGetAllVM
                    {
                        LoanID = b.LoanID,
                        EmployeeID = b.EmployeeID,
                        LoanAmount = b.LoanAmount ?? 0,
                        TenureMonth = b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText ?? "" : "",
                        MonthlyEMI = (b.LoanAmount.HasValue && b.LoanInstallmentPeriod != null && b.LoanInstallmentPeriod.PeriodValue.HasValue)
                         ? b.LoanAmount.Value / b.LoanInstallmentPeriod.PeriodValue.Value : 0,
                        EmployeeName = b.Employee != null ? $"{b.Employee.FirstName ?? ""} {b.Employee.LastName ?? ""}".Trim() : "",
                        EmployeeImage = (b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName)) ? url + b.Employee.EmployeeImageFileName : "",
                        StatusName = b.Status != null ? b.Status.StatusName ?? "" : "",
                        EmployeeDepartment = empoffi.AllActive() .Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department != null ? m.Department.DepartmentName ?? "" : "").FirstOrDefault() ?? ""
                    });


                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>
                {
                    Data = new List<LoanViewGetAllVM>(),
                    TotalCount = 0
                };
            }
        }

        #endregion

        #region Below Table 

        public async Task<PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>> GetAllTableBelowAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name).FirstOrDefaultAsync();
                bool isSuperAdmin = string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
                // 🔹 Step 3: Base query with includes
                var query = loanRepository.AllActive().Include(x => x.Employee).Include(x => x.LoanBaseApprovalHistory).Include(x => x.LoanInstallmentPeriod).OrderByDescending(x => x.LoanID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }
                var officeInfoQuery = empoffi.AllActive().AsQueryable();

                if (organizationId.HasValue)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => x.OrganizationID == organizationId).Select(x => x.EmployeeID).ToListAsync();
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

                //
                Expression<Func<Loan, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.EmployeeID,
                    "empname" => x => x.Employee.FirstName + " " + x.Employee.LastName,  // Employee Name
                    "empdept" => x => empoffi.AllActive().Where(e => e.EmployeeID == x.EmployeeID).Select(m => m.Department != null ? m.Department.DepartmentName : "").FirstOrDefault(),
                    "emploanamount" => x => x.LoanAmount ?? 0,
                    //"empearlypayamount" => x => x.EarlyPayment ?? 0,  // Early Payment
                    "tenuremonth" => x => x.LoanInstallmentPeriod != null ? x.LoanInstallmentPeriod.PeriodText : "",  // Tenure Month
                    "monthlyemi" => x => (x.LoanAmount.HasValue && x.LoanInstallmentPeriod.PeriodValue.HasValue == true)
                                        ? x.LoanAmount.Value / x.LoanInstallmentPeriod.PeriodValue.Value : 0,  // Monthly EMI
                    "outstandindbalance" => x => (x.LoanAmount ?? 0) - ((x.LoanInstallmentPeriod.PeriodValue.HasValue)
                                         ? (x.LoanAmount ?? 0) / x.LoanInstallmentPeriod.PeriodValue.Value * x.LoanInstallmentPeriod.PeriodValue.Value : 0),
                    _ => x => x.LoanID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

                if (!isSuperAdmin)
                {
                    query = query.Where(x => x.LoanBaseApprovalHistory.Any(h => h.ApproveByID == employeeId));
                }

                // For approver Step
                //
                var result = await PaginationService<Loan, LoanViewGetAllVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.LoanID.ToString(), $"%{term}%") ||
                     EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                     EF.Functions.Like(empoffi.AllActive()
                        .Where(e => e.EmployeeID == b.EmployeeID)
                        .Select(m => m.Department != null ? m.Department.DepartmentName : "")
                        .FirstOrDefault(), $"%{term}%") ||
                      EF.Functions.Like((b.LoanAmount ?? 0).ToString(), $"%{term}%") ||
                      //EF.Functions.Like((b.EarlyPayment ?? 0).ToString(), $"%{term}%") ||       
                      EF.Functions.Like(b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "", $"%{term}%") ||
                      EF.Functions.Like(((b.LoanAmount ?? 0) / (b.LoanInstallmentPeriod.PeriodValue ?? 1)).ToString(), $"%{term}%") ||
                      EF.Functions.Like(((b.LoanAmount ?? 0) - (b.LoanInstallmentPeriod.PeriodValue ?? 0)).ToString(), $"%{term}%"),
                    b => new LoanViewGetAllVM
                    {

                        LoanID = b.LoanID,
                        EmployeeID = b.EmployeeID,
                        StatusName = loanBaseHistory.AllActive().Include(x => x.Status).Where(x => x.LoanID == b.LoanID && x.ApproveByID == employeeId).Select(x => x.Status.StatusName).FirstOrDefault(),  //b.Status.StatusName ?? "",
                        LoanAmount = b.LoanAmount ?? 0,
                        TenureMonth = b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "",
                        MonthlyEMI = (b.LoanAmount.HasValue && b.LoanInstallmentPeriod?.PeriodValue.HasValue == true)
                      ? b.LoanAmount.Value / b.LoanInstallmentPeriod.PeriodValue.Value : 0,
                        EmployeeName = b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : "",
                        EmployeeImage = (b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName)) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department != null ? m.Department.DepartmentName : "").FirstOrDefault()
                    });


                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>
                {
                    Data = new List<LoanViewGetAllVM>(),
                    TotalCount = 0
                };
            }
        }
        #endregion

        public async Task<List<CommonSelectVM>> SelectAsync()
        {
            try
            {
                if (employee == null)
                {
                    throw new InvalidOperationException("Employee repository is not initialized.");
                }
                var data = await employee.AllActive()
                    .Select(e => new CommonSelectVM
                    {
                        Id = e.EmployeeID,
                        Name = $"{e.FirstName} {e.LastName}".Trim() 
                    }).Take(50).ToListAsync();
                if (data == null)
                {
                    throw new InvalidOperationException("Failed to retrieve employee data.");
                }
                return data;
            }
            catch (DbUpdateException ex)
            {
                throw new ApplicationException("A database error occurred while fetching employee data.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching employee data.", ex);
            }
        }


        #endregion

        #region Loan Step
        public async Task<List<PayRollLoanStep>> PayRollLoanStep(int id)
        {
            try
            {
                var result = await loanBaseHistory.AllActive()
                 .Where(x => x.LoanID == id)
                 .AsNoTracking()
                 .Select(lb => new PayRollLoanStep
                 {
                     ApprovarNote = lb.ApproverNote ?? string.Empty,
                     ApproverStep = lb.ApprovalStep ?? 0,
                     ApprovarPerson = lb.ApproveBy != null
                         ? lb.ApproveBy.FirstName + " " + lb.ApproveBy.LastName
                         : string.Empty,
                     StatusName = lb.Status != null ? lb.Status.StatusName : string.Empty,
                     ApprovedOrDeclineDate = DateTimeHelpers.FormatDateTime(lb.CreatedAt)
                 })
                 .OrderBy(x => x.ApproverStep)
                 .ToListAsync();



                return result ?? new List<PayRollLoanStep>();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion



    }
}
