using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveApprovalDecline
{



    public class LeaveApprovalService : AppService<LeaveApplications>, ILeaveApprovalService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly ILeaveRequestService leaveRequestService;
        private readonly IGenericRepository<LeaveBalances> leaveBalance;
        private readonly IGenericRepository<LeaveTypes> leaveTypesRepository;
        private readonly IGenericRepository<LeaveBaseApprovalHistory> leaveBaseAprovalHistory;
        private readonly IGenericRepository<Statuses> status;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        private readonly IEmailService emailService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        public LeaveApprovalService(IGenericRepository<LeaveApplications> leaveRequest, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, ILeaveRequestService leaveRequestService, IGenericRepository<LeaveBalances> leaveBalance, IGenericRepository<LeaveTypes> leaveTypesRepository, IGenericRepository<LeaveBaseApprovalHistory> leaveBaseAprovalHistory, IGenericRepository<Statuses> status, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IEmailService emailService, IGenericRepository<Data.Models.Employees> employee) : base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.leaveRequestService = leaveRequestService;
            this.leaveBalance = leaveBalance;

            this.leaveTypesRepository = leaveTypesRepository;
            this.leaveBaseAprovalHistory = leaveBaseAprovalHistory;
            this.status = status;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.approvaldesignation = approvaldesignation;
            this.emailService = emailService;
            this.employee = employee;
        }


        #region  Get Data All  Leave  Requyest above table 



        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>>
    GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                var employeeId = await appDb.Users
                    .Where(u => u.Id == userId)
                    .Select(e => e.EmployeeId)
                    .FirstOrDefaultAsync();

                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name).FirstOrDefaultAsync();

                bool isSuperAdmin = string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

                var query = leaveRequest.AllActive()
                    .Where(x =>x.ApprovalPersonID==employeeId && x.UpdatedBy !=employeeId) 
                    .Include(x => x.Employee)
                    .Include(x => x.LeaveBaseApprovalHistory)
                    .Include(x => x.Status)
                    .Include(x => x.LeaveType)
                    .OrderByDescending(x => x.LeaveApplicationID)
                    .AsQueryable();
                Console.WriteLine("Data" + query);
                if (statusID != null)
                {
                    query = query.Where(x => x.StatusID == statusID);
                }

                if (leaveTypeID != null)
                {
                    query = query.Where(x => x.LeaveTypeID == leaveTypeID);
                }
                if (fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(x => x.FromDate >= fromDate.Value && x.ToDate <= toDate.Value);
                }
                if (!isSuperAdmin)
                {
                    query = query.Where(x => x.ApprovalPersonID == employeeId);
                }

                var leaveBalances = await leaveBalance.AllActive().ToListAsync();
                var leaveTypes = await leaveTypesRepository.AllActive().ToListAsync();
                var employeeDepartments = await empoffi.AllActive()
                    .Include(e => e.Department)
                    .Select(e => new { e.EmployeeID, e.Department.DepartmentName })
                    .ToListAsync();

                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%"),
                    b =>
                    {
                        var balance = leaveBalances.FirstOrDefault(lb =>lb.EmployeeID == b.EmployeeID && lb.LeaveTypeID == b.LeaveTypeID &&  lb.ApplicableYear == b.FromDate.Year);

                        var defaultLeaveDays = leaveTypes.FirstOrDefault(x => x.LeaveTypeID == b.LeaveTypeID)?.LeaveDays ?? 0;

                        var availableLeaveDays = balance != null
                            ? (balance.TotalLeave ?? 0) - (balance.Taken ?? 0 + balance.TakenPartialHours ?? 0): defaultLeaveDays;
                        Console.WriteLine(availableLeaveDays);
                        var department = employeeDepartments.FirstOrDefault(e => e.EmployeeID == b.EmployeeID)?.DepartmentName ?? "";

                        var result = new LeaveApplicationsList
                        {
                            LeaveApplicationID = b.LeaveApplicationID,
                            StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                            IsFullDay = b.IsFullDay,
                            LeaveType = b.LeaveType?.LeaveTypeName ?? "",
                            FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                            ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                            Period = b.IsFullDay
                                ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1
                                : b.PartialFromTime.HasValue && b.PartialToTime.HasValue
                                    ? LeaveCalculationHelper.CalculatePartialHoursTable(b.PartialToTime.Value, b.PartialFromTime.Value)
                                    : 0,
                            EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                            EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                            EmployeeDepartment = department,
                            AvailableLeaveDays = availableLeaveDays
                        };
                        return result;
                    });

                return result;  //result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>
                {
                    Data = new List<LeaveApplicationsList>(),
                    TotalCount = 0
                };
            }
        }

        #endregion

        #region  Get Data All  Leave  Requyest below table 
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableBelowAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, DateOnly? fromDate = null, DateOnly? toDate = null)
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
                var query = leaveRequest.AllActive().Where(x=>x.LeaveBaseApprovalHistory.Any(h => h.ApproveBy == employeeId)) 
                    .Include(x => x.Employee)
                    .Include(x => x.Status)
                    .Include(x => x.LeaveType)
                    .Include(x => x.LeaveBaseApprovalHistory)
                    .OrderByDescending(x => x.LeaveApplicationID).AsQueryable();
                //

                var a = leaveRequest.AllActive();

                var b = a.Count();
                Console.WriteLine(b);

                if (statusID != null)
                {
                    query = query.Where(x => x.StatusID == statusID);
                }

                if (leaveTypeID != null)
                {
                    query = query.Where(x => x.LeaveTypeID == leaveTypeID);
                }

                if (fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(x => x.FromDate >= fromDate.Value && x.ToDate <= toDate.Value);
                }
                if (query == null)
                {
                    throw new InvalidOperationException("ActionLogs query source is null.");
                }
                // 🔹 Step 4: Filter if not SuperAdmin
                if (!isSuperAdmin)
                {
                    query = query.Where(x => x.LeaveBaseApprovalHistory.Any(h => h.ApproveBy == employeeId));
                }

                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%")
                                 ,


                    b => new LeaveApplicationsList
                    {
                        //UserType = b.ActionLogID,
                        LeaveApplicationID = b.LeaveApplicationID,
                        StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                        IsFullDay = b.IsFullDay,
                        LeaveType = b.LeaveType != null ? b.LeaveType.LeaveTypeName : "",
                        FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        Period = b.IsFullDay ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1 : b.PartialFromTime.HasValue && b.PartialToTime.HasValue ? LeaveCalculationHelper.CalculatePartialHoursTable(b.PartialToTime.Value, b.PartialFromTime.Value) : 0,
                        EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),

                        //
                    });


                return result;


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>
                {
                    Data = new List<LeaveApplicationsList>(),
                    TotalCount = 0
                };
            }
        }



        #endregion

        #region Get Data By LeaveRequestID



        public async Task<LeaveApplicationApprovalModifyVM> GetLeaveRequestByIdAsync(int leaveApplicationID)
        {
            if (leaveApplicationID == 0)
            {
                return null;
            }

            await leaveRequest.BeginTransactionAsync();
            try
            {
                var data = await leaveRequest.GetByIdAsync(leaveApplicationID);
                if (data == null) return null;
                //
                SubsequentVM subsequent = null;

                var fromDateTime = data.FromDate.ToDateTime(TimeOnly.MinValue);
                var toDateTime = data.ToDate.ToDateTime(TimeOnly.MinValue);

                subsequent = await leaveRequestService.SubsequentAsynce(fromDateTime, toDateTime);

                // avilable days 
                int applicableYear = data.FromDate.Year;

                var leaveBalancevaluse = await leaveBalance.AllActive().FirstOrDefaultAsync(x => x.EmployeeID == data.EmployeeID && x.LeaveTypeID == data.LeaveTypeID || x.ApplicableYear == applicableYear);
                var leaveType = await leaveTypesRepository.AllActive().FirstOrDefaultAsync(x => x.LeaveTypeID == data.LeaveTypeID);
                decimal availableLeaveDays = 0;
                if (leaveBalancevaluse != null && leaveBalancevaluse.TotalLeave.HasValue && leaveBalancevaluse.Taken.HasValue)
                {
                    availableLeaveDays = leaveBalancevaluse.TotalLeave.Value - leaveBalancevaluse.Taken.Value;
                }
                else
                {
                    availableLeaveDays = leaveType?.LeaveDays ?? 0;
                }
                //
                LeaveApplicationApprovalModifyVM entityVM = new LeaveApplicationApprovalModifyVM
                {
                    LeaveApplicationID = data.LeaveApplicationID,
                    EmployeeIDEdit = data.EmployeeID,
                    LeaveTypeIDEdit = data.LeaveTypeID,
                    ReasonEdit = data.Reason,
                    IsFullDayEdit = data.IsFullDay,
                    FromDateEdit = data.FromDate,
                    ToDateEdit = data.ToDate,
                    PartialFromTimeEdit = data.PartialFromTime,
                    PartialToTimeEdit = data.PartialToTime,
                    Period = data.IsFullDay ? (data.ToDate.DayNumber - data.FromDate.DayNumber) + 1 : data.PartialFromTime.HasValue && data.PartialToTime.HasValue ? (int)(data.PartialToTime.Value - data.PartialFromTime.Value).TotalHours : 0,
                    TotalSubsequentDays = subsequent?.TotalSubsequentDays,
                    IsHolidayCountedAsLeave = subsequent?.IsHolidayCountedAsLeave ?? false,
                    IsWeekendCountedAsLeave = subsequent?.IsWeekendCountedAsLeave ?? false,
                    AvailableLeaveDays = availableLeaveDays,

                };
                return entityVM;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        #endregion

        #region  Update Method

        private async Task<int?> GetIdByNameAsync(string name)
        {
            var data = await status.AllActive().Where(x => EF.Functions.Like(x.StatusName.ToLower(), name.ToLower())).Select(x => (int?)x.StatusID).FirstOrDefaultAsync();

            return data;
        }

        // Server side Validation DateRange 

        private bool ValidateLeaveDates(LeaveApplicationApprovalModifyVM model, LeaveApplications entity, out List<string> errors)
        {
            errors = new List<string>();

            var minDate = entity.FromDate;
            var maxDate = entity.ToDate;
            if (model.IsFullDayEdit)
            {
                if (!(minDate <= model.FromDateEdit && model.FromDateEdit <= maxDate))
                    errors.Add("From Date must be within the allowed range.");

                if (!(minDate <= model.ToDateEdit && model.ToDateEdit <= maxDate))
                    errors.Add("To Date must be within the allowed range.");
            }
            return !errors.Any();
        }

        // Time Hour Min
        public static decimal CalculatePartialHours(TimeOnly? from, TimeOnly? to)
        {
            if (!from.HasValue || !to.HasValue)
                return 0;

            var duration = to.Value.ToTimeSpan() - from.Value.ToTimeSpan();

            if (duration.TotalMinutes <= 0)
                return 0;
            var result = Math.Round((decimal)duration.TotalMinutes / 60, 2); // e.g., 1.67
            return result;
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
        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationApprovalModifyVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data cannot be null"
                };
            }

            await leaveRequest.BeginTransactionAsync();

            try

            {
                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
                if (entity == null)
                    return null;

                // Sever side Validation For Date range 
                if (!ValidateLeaveDates(entityVM, entity, out var errors))
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = string.Join(" ", errors)
                    };
                }
                //
                // Get employee office info
                var offf = await empoffi.AllActive()
                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
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
                        (x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID) && x.ApprovalType.ApprovalTypeName == "Leave Request Approval");
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

                // 🔹 Update full-day or partial-day
                entity.IsFullDay = entityVM.IsFullDayEdit;
                if (entityVM.IsFullDayEdit)
                {
                    entity.FromDate = entityVM.FromDateEdit ?? default;
                    entity.ToDate = entityVM.ToDateEdit ?? default;
                    entity.PartialFromTime = null;
                    entity.PartialToTime = null;
                }
                else if (entityVM.ToDateFromDateCombinedEdit.HasValue)
                {
                    var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
                    entity.FromDate = dateOnly;
                    entity.ToDate = dateOnly;
                    entity.PartialFromTime = entityVM.PartialFromTimeEdit;
                    entity.PartialToTime = entityVM.PartialToTimeEdit;
                }
                // 🔹 Update leave balance only if final approval (CHANGED)
                if (isFinalApproval && entityVM.Approved == true)
                {
                    var leaveDaysFromConfig = await leaveTypesRepository.AllActive()
                        .Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit)
                        .Select(x => new { x.LeaveDays, x.ApplicableYear })
                        .FirstOrDefaultAsync();

                    var existingBalance = await leaveBalance.AllActive()
                        .FirstOrDefaultAsync(x =>
                            x.EmployeeID == entityVM.EmployeeIDEdit &&
                            x.LeaveTypeID == entityVM.LeaveTypeIDEdit);

                    if (existingBalance != null)
                    {
                        if (entityVM.IsFullDayEdit)
                        {
                            existingBalance.Taken = (existingBalance.Taken ?? 0) + entityVM.TotalAppliedDays;
                        }
                        else
                        {
                            var newPartial = CalculatePartialHours(entityVM.PartialFromTimeEdit, entityVM.PartialToTimeEdit);
                            existingBalance.TakenPartialHours = (existingBalance.TakenPartialHours ?? 0) + newPartial;
                        }
                        existingBalance.TotalLeave = leaveDaysFromConfig.LeaveDays;
                        existingBalance.ApplicableYear = leaveDaysFromConfig.ApplicableYear;
                        existingBalance.LIP = entityVM.LIP;
                        existingBalance.LMAC = entityVM.LMAC;
                        existingBalance.UpdatedAt = DateTime.Now;
                        existingBalance.UpdatedBy = entityVM.UpdatedBy;
                        await leaveBalance.UpdateAsync(existingBalance);
                    }
                    else
                    {
                        var newBalance = new LeaveBalances
                        {
                            EmployeeID = entityVM.EmployeeIDEdit,
                            LeaveTypeID = entityVM.LeaveTypeIDEdit,
                            Taken = entityVM.IsFullDayEdit ? entityVM.TotalAppliedDays : 0,
                            TakenPartialHours = entityVM.IsFullDayEdit ? 0 : CalculatePartialHours(entityVM.PartialFromTimeEdit, entityVM.PartialToTimeEdit),
                            TotalLeave = leaveDaysFromConfig.LeaveDays,
                            ApplicableYear = leaveDaysFromConfig.ApplicableYear,
                            CreatedAt = DateTime.Now,
                            CreatedBy = entityVM.CreatedBy,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC
                        };

                        await leaveBalance.AddAsync(newBalance);
                    }

                }
                entity.ApprovalPersonID = approvalPersonId;
                entity.StatusID = statusId;
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = entityVM.UpdatedBy;
                await leaveRequest.UpdateAsync(entity);

                // 🔹 Always save approval history
                var leaveBase = new LeaveBaseApprovalHistory
                {
                    LeaveApplicationID = entityVM.LeaveApplicationID,
                    StatusID = statusId,
                    ApproverNote = entityVM.ApprovalNote,
                    LeaveTypeID = entityVM.LeaveTypeIDEdit,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    ApproveBy = entityVM.CreatedBy,
                    ApprovalStep = approvalStep
                };

                if (entityVM.IsFullDayEdit)
                {
                    leaveBase.FromDate = entityVM.FromDateEdit ?? default;
                    leaveBase.ToDate = entityVM.ToDateEdit ?? default;
                }
                else if (entityVM.ToDateFromDateCombinedEdit.HasValue)
                {
                    var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
                    leaveBase.FromDate = dateOnly;
                    leaveBase.ToDate = dateOnly;
                    leaveBase.PartialFromTime = entityVM.PartialFromTimeEdit;
                    leaveBase.PartialToTime = entityVM.PartialToTimeEdit;
                }
                await leaveBaseAprovalHistory.AddAsync(leaveBase);


                // for email
                var approvalDepartment = await empoffi.AllActive().Where(x => x.EmployeeID == approvalPersonId)
            .Select(x => new { x.OfficeEmail, x.Department.DepartmentName, x.Designation.DesignationName }).FirstOrDefaultAsync();
                var applicantNameEmail = await employee.AllActive()
                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
                    .Select(x => new { x.FirstName, x.LastName, x.Email }).FirstOrDefaultAsync();

                var applicantDepartment = await empoffi.AllActive()
                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
                    .Select(x => new
                    {
                        x.OfficeEmail,
                        DepartmentName = x.Department.DepartmentName,
                        DesignationName = x.Designation.DesignationName
                    }).FirstOrDefaultAsync();

                // Approver info
                var approverNameEmail = await employee.AllActive()
                    .Where(x => x.EmployeeID == approvalPersonId)
                    .Select(x => new { x.FirstName, x.LastName, x.Email })
                    .FirstOrDefaultAsync();

                var approverDepartment = await empoffi.AllActive()
                    .Where(x => x.EmployeeID == approvalPersonId)
                    .Select(x => new
                    {
                        x.OfficeEmail,
                        DepartmentName = x.Department.DepartmentName,
                        DesignationName = x.Designation.DesignationName
                    }).FirstOrDefaultAsync();
                var leaveName = await leaveTypesRepository.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
                // Calculate total days (inclusive)
                // Calculate total days (inclusive)
                int totalDays = 0;

                if (entityVM.FromDateEdit.HasValue && entityVM.ToDateEdit.HasValue)
                {
                    totalDays = (entityVM.ToDateEdit.Value.DayNumber - entityVM.FromDateEdit.Value.DayNumber) + 1;
                }
                string toEmail;

                if (statusId == leavStatusDecline)
                {
                    // Notify applicant after decision
                    toEmail = applicantNameEmail?.Email ?? applicantDepartment?.OfficeEmail;
                }
                else
                {
                    // Fallback (new leave application goes to approver)
                    toEmail = approverNameEmail?.Email ?? approverDepartment?.OfficeEmail;
                }

                // Build email model
                var emailModel = new EmailVM
                {
                    To = toEmail, // fallback if personal email is null
                    Subject = $"Leave Application from {applicantNameEmail?.FirstName} {applicantNameEmail?.LastName}",
                    Body = $@"
        <p>Dear {approverNameEmail?.FirstName} {approverNameEmail?.LastName},</p>
        <p>{applicantNameEmail?.FirstName} {applicantNameEmail?.LastName} 
        ({applicantDepartment?.DesignationName}, {applicantDepartment?.DepartmentName}) 
        has applied for leave.</p>

        <ul>
            <li><strong>From:</strong> {entityVM.FromDateEdit:dd MMM yyyy}</li>
            <li><strong>To:</strong> {entityVM.ToDateEdit:dd MMM yyyy}</li>
            
           <li><strong>Total day(s):</strong> {totalDays}</li> 
           <li><strong>Leave Type:</strong> {leaveName}</li> 
            <li><strong>Reason:</strong> {entityVM.ReasonEdit}</li>
        </ul>

        <p>Please log in to the HRM system to review and approve this request.</p>
         <p>
    <a href='' style='padding:8px 12px;background:#007bff;color:#fff;text-decoration:none;border-radius:4px;'>Login</a>
    &nbsp;&nbsp;
    <a href='' style='padding:8px 12px;background:#28a745;color:#fff;text-decoration:none;border-radius:4px;'>Accept</a>
    &nbsp;&nbsp;
    <a href='' style='padding:8px 12px;background:#dc3545;color:#fff;text-decoration:none;border-radius:4px;'>Decline</a>
</p>
        <p>Regards,<br/>HRM System</p>
    "
                };

                // Send using EmailService (SMTP uses applicant’s org config)
                await emailService.SendEmailAsync(emailModel, entityVM.EmployeeIDEdit);

                await leaveRequest.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated Successfully."
                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave request Update."
                };
            }
        }


        #endregion

        #region Dispaly LeaveDays 
        public async Task<List<LeaveBalancesDisplayVM>> GetLeaveTypeBalancesForEmployee(string userId)
        {
            // Get employee ID from user ID
            var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();

            if (employeeId == null)
                return new List<LeaveBalancesDisplayVM>(); // or throw exception if required

            // Get the role of the user
            var roleName = await (
                from user in appDb.Users
                join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                join role in appDb.Roles on userRole.RoleId equals role.Id
                where user.Id == userId
                select role.Name
            ).FirstOrDefaultAsync();

            // Base query for leave balances
            var baseQuery = from lt in leaveTypesRepository.AllActive()
                            join lb in leaveBalance.AllActive().Where(x => x.EmployeeID == employeeId)
                                on lt.LeaveTypeID equals lb.LeaveTypeID into lbGroup
                            from lb in lbGroup.DefaultIfEmpty()
                            select new LeaveBalancesDisplayVM
                            {
                                LeaveBalanceID = lb != null ? lb.LeaveBalanceID : 0,
                                EmployeeID = employeeId,
                                LeaveTypeID = lt.LeaveTypeID,
                                LeaveTypeName = lt.LeaveTypeName,
                                TotalLeave = lb.TotalLeave ?? lt.LeaveDays,
                                Taken = lb.Taken,
                                ApplicableYear = lb.ApplicableYear,
                                RemainingDays = lb != null
                                    ? ((lb.TotalLeave ?? 0) - ((lb.Taken ?? 0) + (lb.TakenPartialHours ?? 0)))
                                    : (lt.LeaveDays ?? 0)
                            };
           return await baseQuery.ToListAsync();



            #endregion
        }
    }
    }
