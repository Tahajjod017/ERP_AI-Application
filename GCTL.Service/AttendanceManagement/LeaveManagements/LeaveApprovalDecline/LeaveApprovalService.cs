using GCTL.Core.Helpers.CommonSelectMasterDropDown;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
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
using static GCTL.Service.AdminSettings.GeneralSettings.UtcTimeHelper;
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
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly ICommonDroDownService commonDroDownService;
        private readonly IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration;
        private readonly IGenericRepository<Statuses> leaveStatuses;
        private readonly ILocalizationContext _localizationContext;
        public LeaveApprovalService(IGenericRepository<LeaveApplications> leaveRequest, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, ILeaveRequestService leaveRequestService, IGenericRepository<LeaveBalances> leaveBalance, IGenericRepository<LeaveTypes> leaveTypesRepository, IGenericRepository<LeaveBaseApprovalHistory> leaveBaseAprovalHistory, IGenericRepository<Statuses> status, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IEmailService emailService, IGenericRepository<Data.Models.Employees> employee, IGenericRepository<Organization> organizationRepository, ICommonDroDownService commonDroDownService, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration, IGenericRepository<Statuses> leaveStatuses, ILocalizationContext localizationContext) : base(leaveRequest)
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
            _organizationRepository = organizationRepository;
            this.commonDroDownService = commonDroDownService;
            this.leavePolicyConfiguration = leavePolicyConfiguration;
            this.leaveStatuses = leaveStatuses;
            _localizationContext = localizationContext;
        }

        #region Leave Type 
        public bool IsEligibleForLeave(LeaveTypes leaveType, DateOnly joiningDate, DateOnly today)
        {
            // Case 1: EffectiveFrom + EffectiveAfter = Years
            if (leaveType.EffectiveAfter?.Contains("Year") == true && leaveType.EffectiveFrom.HasValue)
            {
                var eligibleDate = joiningDate.AddYears(leaveType.EffectiveFrom.Value);
                return today >= eligibleDate;
            }

            // Case 2: EffectiveFrom + EffectiveAfter = Months
            if (leaveType.EffectiveAfter?.Contains("Month") == true && leaveType.EffectiveFrom.HasValue)
            {
                var eligibleDate = joiningDate.AddMonths(leaveType.EffectiveFrom.Value);
                return today >= eligibleDate;
            }

            // Case 3: EffectiveFromMonthYear (e.g. "2025-01")
            if (!string.IsNullOrEmpty(leaveType.EffectiveFromMonthYear))
            {
                if (DateTime.TryParse(leaveType.EffectiveFromMonthYear + "-01", out var effDate))
                {
                    var effDateOnly = DateOnly.FromDateTime(effDate);
                    return today >= effDateOnly;
                }
            }

            // Case 4: After Joining Date (immediate eligibility)
            if (leaveType.EffectiveAfter?.Contains("After Joining Date") == true)
            {
                return today >= joiningDate;
            }

            // Default: eligible if nothing blocks it
            return true;
        }


        #endregion

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
                    availableLeaveDays = leaveBalancevaluse.TotalLeave.Value - (leaveBalancevaluse.Taken.Value+leaveBalancevaluse.TakenPartialHours.Value );
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
                    ApprovalPersonID = data.ApprovalPersonID,
                    SecrectCode = data.SecrectCode,
                    SecrectCodeDateTime = data.SecrectCodeDateTime,
                    Taken= leaveBalancevaluse?.Taken ?? 0
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
        #region Old Code 
        //        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationApprovalModifyVM entityVM)
        //        {
        //            if (entityVM == null)
        //            {
        //                return new CommonReturnViewModel
        //                {
        //                    Success = false,
        //                    Message = "Data cannot be null"
        //                };
        //            }

        //            await leaveRequest.BeginTransactionAsync();

        //            try

        //            {
        //                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
        //                if (entity == null)
        //                    return null;

        //                // Sever side Validation For Date range 
        //                if (!ValidateLeaveDates(entityVM, entity, out var errors))
        //                {
        //                    return new CommonReturnViewModel
        //                    {
        //                        Success = false,
        //                        Message = string.Join(" ", errors)
        //                    };
        //                }
        //                //
        //                // Get employee office info
        //                var offf = await empoffi.AllActive()
        //                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
        //                    .Select(x => new { x.EmployeeID, x.OrganizationID, x.OrganizationBranchID, x.DepartmentID, x.DesignationID, x.ImmediateSupervisorId, x.SeniorSupervisorId, x.HeadOfDepartmentId }).FirstOrDefaultAsync();

        //                if (offf == null)
        //                {
        //                    return new CommonReturnViewModel
        //                    {
        //                        Success = false,
        //                        Message = "Employee office info not found."
        //                    };
        //                }
        //                // Get approval settings
        //                var approvalSettings = await approvalSettingsRepository.AllActive().Include(x => x.ApprovalType).FirstOrDefaultAsync(x =>
        //                        (x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID) && x.ApprovalType.ApprovalTypeName == "Leave Request Approval");
        //                if (approvalSettings == null)
        //                {
        //                    return new CommonReturnViewModel
        //                    {
        //                        Success = false,
        //                        Message = "Your Company Does not Exists in Approver Settings."
        //                    };
        //                }
        //                //


        //                //
        //                var approvalFlow = new List<(int? id, bool isDesignation)>
        //                    {
        //                        (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
        //                        (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
        //                        (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
        //                    };

        //                bool isFinalApproval = false;
        //                bool isFirstApprover = false;
        //                bool isSecondApprover = false;
        //                bool isThirdApprover = false;
        //                bool allowSelfApprover = false;
        //                int? approvalPersonId = null;
        //                if (!approvalSettings.IsDesignationOrEmpFirstApprovalID || !approvalSettings.IsDesignationOrEmpSecondApprovalID || !approvalSettings.IsDesignationOrEmpThirdApprovalID)
        //                {
        //                    isFirstApprover = approvalSettings != null && approvalSettings?.FirstApprovalID == entityVM.UpdatedBy;
        //                    isSecondApprover = approvalSettings != null && approvalSettings?.SecondApprovalID == entityVM.UpdatedBy;
        //                    isThirdApprover = approvalSettings != null && approvalSettings?.ThirdApprovalID == entityVM.UpdatedBy;
        //                    allowSelfApprover = approvalSettings != null && approvalSettings.SelfExceptionApprovalID == entityVM.UpdatedBy;
        //                }
        //                //else if (approvalSettings.IsDesignationOrEmpFirstApprovalID || approvalSettings.IsDesignationOrEmpSecondApprovalID || approvalSettings.IsDesignationOrEmpThirdApprovalID)
        //                //{

        //                //    int? resolvedFirst = await ResolveApprovalAsync(approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID, offf);
        //                //    int? resolvedSecond = await ResolveApprovalAsync(approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID, offf);
        //                //    int? resolvedThird = await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf);
        //                //    isFirstApprover = resolvedFirst == entityVM.UpdatedBy;
        //                //    isSecondApprover = resolvedSecond == entityVM.UpdatedBy;
        //                //    isThirdApprover = resolvedThird == entityVM.UpdatedBy;
        //                //    if (isFirstApprover)
        //                //    {
        //                //        approvalPersonId = resolvedSecond;
        //                //    }
        //                //    else if (isSecondApprover)
        //                //    {
        //                //        approvalPersonId = resolvedThird;
        //                //    }
        //                //    else
        //                //    {
        //                //        approvalPersonId = resolvedThird;
        //                //        if (entityVM.Approved)
        //                //        {
        //                //            isFinalApproval = true;

        //                //        }
        //                //    }
        //                //}

        //                //
        //                int approvalStep = 0;
        //                if (approvalSettings != null)
        //                {
        //                    // resolve approvers using helper
        //                    int? resolvedFirst = await ResolveApprovalAsync(approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID, offf);
        //                    int? resolvedSecond = await ResolveApprovalAsync(approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID, offf);
        //                    int? resolvedThird = await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf);

        //                    // check who the current approver is
        //                    isFirstApprover = resolvedFirst == entityVM.UpdatedBy;
        //                    isSecondApprover = resolvedSecond == entityVM.UpdatedBy;
        //                    isThirdApprover = resolvedThird == entityVM.UpdatedBy;

        //                    // figure out which approvers are enabled
        //                    bool hasFirst = approvalSettings.FirstApprovalID.HasValue && approvalSettings.IsDesignationOrEmpFirstApprovalID;
        //                    bool hasSecond = approvalSettings.SecondApprovalID.HasValue && approvalSettings.IsEnableSecondApproval;
        //                    bool hasThird = approvalSettings.ThirdApprovalID.HasValue && approvalSettings.IsEnableThirdApproval;

        //                    // determine if this is the final step
        //                    bool isFinalStep =
        //                        (isFirstApprover && !hasSecond && !hasThird) ||   // only 1st exists
        //                        (isSecondApprover && !hasThird) ||                // only 2nd OR 1st+2nd (no 3rd)
        //                        (isThirdApprover);                                // always final if 3rd exists

        //                    if (entityVM.Approved && isFinalStep)
        //                    {
        //                        isFinalApproval = true;
        //                    }

        //                    // set next approver
        //                    if (isFirstApprover)
        //                    {
        //                        approvalPersonId = resolvedSecond;
        //                    }
        //                    else if (isSecondApprover)
        //                    {
        //                        approvalPersonId = resolvedThird;
        //                    }
        //                    else if (isThirdApprover)
        //                    {
        //                        approvalPersonId = resolvedThird; // last in chain
        //                    }

        //                    // approval step tracking
        //                    approvalStep = isFirstApprover ? 1 :
        //                                   isSecondApprover ? 2 :
        //                                   isThirdApprover ? 3 : 0;
        //                }
        //                //

        //                if (approvalSettings != null && !approvalSettings.IsDesignationOrEmpFirstApprovalID &&
        //                    !approvalSettings.IsDesignationOrEmpSecondApprovalID && !approvalSettings.IsDesignationOrEmpThirdApprovalID)
        //                {
        //                    approvalStep = isFirstApprover ? 1 : isSecondApprover ? 2 : isThirdApprover ? 3 : allowSelfApprover ? 4 : 0;
        //                }
        //                else if (approvalSettings != null && (approvalSettings.IsDesignationOrEmpFirstApprovalID ||
        //                         approvalSettings.IsDesignationOrEmpSecondApprovalID ||
        //                         approvalSettings.IsDesignationOrEmpThirdApprovalID))
        //                {

        //                    approvalStep = isFirstApprover ? 1 : isSecondApprover ? 2 : isThirdApprover ? 3 : 0;

        //                }
        //                if (!isFirstApprover && !isSecondApprover && !isThirdApprover && !allowSelfApprover)
        //                {
        //                    return new CommonReturnViewModel
        //                    {
        //                        Success = false,
        //                        Message = "You are not authorized to approve this leave request."
        //                    };
        //                }
        //                // Get status IDs
        //                int? leavStatusApproved = await GetIdByNameAsync("APPROVED");
        //                int? leavStatusDecline = await GetIdByNameAsync("DECLINED");
        //                int? statusId = entityVM.Approved ? leavStatusApproved : leavStatusDecline;
        //                if (!statusId.HasValue)
        //                {
        //                    return new CommonReturnViewModel
        //                    {
        //                        Success = false,
        //                        Message = "Approval or Decline must be selected."
        //                    };
        //                }

        //                // 🔹 Authorization chec

        //                if (isFirstApprover && approvalSettings.IsEnableSecondApproval && !approvalSettings.IsDesignationOrEmpFirstApprovalID)
        //                {

        //                    approvalPersonId = approvalSettings.SecondApprovalID;
        //                }
        //                else if (isSecondApprover && approvalSettings.IsEnableThirdApproval && !approvalSettings.IsDesignationOrEmpSecondApprovalID)
        //                {
        //                    approvalPersonId = approvalSettings.ThirdApprovalID;
        //                }
        //                else if (isThirdApprover && !approvalSettings.IsDesignationOrEmpThirdApprovalID)
        //                {
        //                    approvalPersonId = approvalSettings.ThirdApprovalID;
        //                    if (entityVM.Approved)
        //                    {
        //                        isFinalApproval = true;
        //                    }

        //                }
        //                else if (allowSelfApprover && approvalSettings.AllowSelfApproval.HasValue && !approvalSettings.AllowSelfApproval.Value)
        //                {
        //                    approvalPersonId = approvalSettings.SelfExceptionApprovalID;
        //                    if (entityVM.Approved)
        //                    {
        //                        isFinalApproval = true;
        //                    }

        //                }

        //                // 🔹 Update full-day or partial-day
        //                entity.IsFullDay = entityVM.IsFullDayEdit;
        //                if (entityVM.IsFullDayEdit)
        //                {
        //                    entity.FromDate = entityVM.FromDateEdit ?? default;
        //                    entity.ToDate = entityVM.ToDateEdit ?? default;
        //                    entity.PartialFromTime = null;
        //                    entity.PartialToTime = null;
        //                }
        //                else if (entityVM.ToDateFromDateCombinedEdit.HasValue)
        //                {
        //                    var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
        //                    entity.FromDate = dateOnly;
        //                    entity.ToDate = dateOnly;
        //                    entity.PartialFromTime = entityVM.PartialFromTimeEdit;
        //                    entity.PartialToTime = entityVM.PartialToTimeEdit;
        //                }
        //                // 🔹 Update leave balance only if final approval (CHANGED)
        //                if (isFinalApproval && entityVM.Approved == true)
        //                {
        //                    var leaveDaysFromConfig = await leaveTypesRepository.AllActive()
        //                        .Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit)
        //                        .Select(x => new { x.LeaveDays, x.ApplicableYear })
        //                        .FirstOrDefaultAsync();

        //                    var existingBalance = await leaveBalance.AllActive()
        //                        .FirstOrDefaultAsync(x =>
        //                            x.EmployeeID == entityVM.EmployeeIDEdit &&
        //                            x.LeaveTypeID == entityVM.LeaveTypeIDEdit);

        //                    if (existingBalance != null)
        //                    {
        //                        if (entityVM.IsFullDayEdit)
        //                        {
        //                            existingBalance.Taken = (existingBalance.Taken ?? 0) + entityVM.TotalAppliedDays;
        //                        }
        //                        else
        //                        {
        //                            var newPartial = CalculatePartialHours(entityVM.PartialFromTimeEdit, entityVM.PartialToTimeEdit);
        //                            existingBalance.TakenPartialHours = (existingBalance.TakenPartialHours ?? 0) + newPartial;
        //                        }
        //                        existingBalance.TotalLeave = leaveDaysFromConfig.LeaveDays;
        //                        existingBalance.ApplicableYear = leaveDaysFromConfig.ApplicableYear;
        //                        existingBalance.LIP = entityVM.LIP;
        //                        existingBalance.LMAC = entityVM.LMAC;
        //                        existingBalance.UpdatedAt = DateTime.Now;
        //                        existingBalance.UpdatedBy = entityVM.UpdatedBy;
        //                        await leaveBalance.UpdateAsync(existingBalance);
        //                    }
        //                    else
        //                    {
        //                        var newBalance = new LeaveBalances
        //                        {
        //                            EmployeeID = entityVM.EmployeeIDEdit,
        //                            LeaveTypeID = entityVM.LeaveTypeIDEdit,
        //                            Taken = entityVM.IsFullDayEdit ? entityVM.TotalAppliedDays : 0,
        //                            TakenPartialHours = entityVM.IsFullDayEdit ? 0 : CalculatePartialHours(entityVM.PartialFromTimeEdit, entityVM.PartialToTimeEdit),
        //                            TotalLeave = leaveDaysFromConfig.LeaveDays,
        //                            ApplicableYear = leaveDaysFromConfig.ApplicableYear,
        //                            CreatedAt = DateTime.Now,
        //                            CreatedBy = entityVM.CreatedBy,
        //                            LIP = entityVM.LIP,
        //                            LMAC = entityVM.LMAC
        //                        };

        //                        await leaveBalance.AddAsync(newBalance);
        //                    }

        //                }
        //                entity.ApprovalPersonID = approvalPersonId;
        //                entity.StatusID = statusId;
        //                entity.LIP = entityVM.LIP;
        //                entity.LMAC = entityVM.LMAC;
        //                entity.UpdatedAt = DateTime.Now;
        //                entity.UpdatedBy = entityVM.UpdatedBy;
        //                await leaveRequest.UpdateAsync(entity);

        //                // 🔹 Always save approval history
        //                var leaveBase = new LeaveBaseApprovalHistory
        //                {
        //                    LeaveApplicationID = entityVM.LeaveApplicationID,
        //                    StatusID = statusId,
        //                    ApproverNote = entityVM.ApprovalNote,
        //                    LeaveTypeID = entityVM.LeaveTypeIDEdit,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = entityVM.CreatedBy,
        //                    LIP = entityVM.LIP,
        //                    LMAC = entityVM.LMAC,
        //                    ApproveBy = entityVM.CreatedBy,
        //                    ApprovalStep = approvalStep
        //                };

        //                if (entityVM.IsFullDayEdit)
        //                {
        //                    leaveBase.FromDate = entityVM.FromDateEdit ?? default;
        //                    leaveBase.ToDate = entityVM.ToDateEdit ?? default;
        //                }
        //                else if (entityVM.ToDateFromDateCombinedEdit.HasValue)
        //                {
        //                    var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
        //                    leaveBase.FromDate = dateOnly;
        //                    leaveBase.ToDate = dateOnly;
        //                    leaveBase.PartialFromTime = entityVM.PartialFromTimeEdit;
        //                    leaveBase.PartialToTime = entityVM.PartialToTimeEdit;
        //                }
        //                await leaveBaseAprovalHistory.AddAsync(leaveBase);


        //                // for email
        //                var approvalDepartment = await empoffi.AllActive().Where(x => x.EmployeeID == approvalPersonId)
        //            .Select(x => new { x.OfficeEmail, x.Department.DepartmentName, x.Designation.DesignationName }).FirstOrDefaultAsync();
        //                var applicantNameEmail = await employee.AllActive()
        //                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
        //                    .Select(x => new { x.FirstName, x.LastName, x.Email }).FirstOrDefaultAsync();

        //                var applicantDepartment = await empoffi.AllActive()
        //                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
        //                    .Select(x => new
        //                    {
        //                        x.OfficeEmail,
        //                        DepartmentName = x.Department.DepartmentName,
        //                        DesignationName = x.Designation.DesignationName
        //                    }).FirstOrDefaultAsync();

        //                // Approver info
        //                var approverNameEmail = await employee.AllActive()
        //                    .Where(x => x.EmployeeID == approvalPersonId)
        //                    .Select(x => new { x.FirstName, x.LastName, x.Email })
        //                    .FirstOrDefaultAsync();

        //                var approverDepartment = await empoffi.AllActive()
        //                    .Where(x => x.EmployeeID == approvalPersonId)
        //                    .Select(x => new
        //                    {
        //                        x.OfficeEmail,
        //                        DepartmentName = x.Department.DepartmentName,
        //                        DesignationName = x.Designation.DesignationName
        //                    }).FirstOrDefaultAsync();
        //                var leaveName = await leaveTypesRepository.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
        //                // Calculate total days (inclusive)
        //                // Calculate total days (inclusive)
        //                int totalDays = 0;

        //                if (entityVM.FromDateEdit.HasValue && entityVM.ToDateEdit.HasValue)
        //                {
        //                    totalDays = (entityVM.ToDateEdit.Value.DayNumber - entityVM.FromDateEdit.Value.DayNumber) + 1;
        //                }
        //                string toEmail;

        //                if (statusId == leavStatusDecline)
        //                {
        //                    // Notify applicant after decision
        //                    toEmail = applicantNameEmail?.Email ?? applicantDepartment?.OfficeEmail;
        //                }
        //                else
        //                {
        //                    // Fallback (new leave application goes to approver)
        //                    toEmail = approverNameEmail?.Email ?? approverDepartment?.OfficeEmail;
        //                }

        //                // Build email model
        //                var emailModel = new EmailVM
        //                {
        //                    To = toEmail, // fallback if personal email is null
        //                    Subject = $"Leave Application from {applicantNameEmail?.FirstName} {applicantNameEmail?.LastName}",
        //                    Body = $@"
        //        <p>Dear {approverNameEmail?.FirstName} {approverNameEmail?.LastName},</p>
        //        <p>{applicantNameEmail?.FirstName} {applicantNameEmail?.LastName} 
        //        ({applicantDepartment?.DesignationName}, {applicantDepartment?.DepartmentName}) 
        //        has applied for leave.</p>

        //        <ul>
        //            <li><strong>From:</strong> {entityVM.FromDateEdit:dd MMM yyyy}</li>
        //            <li><strong>To:</strong> {entityVM.ToDateEdit:dd MMM yyyy}</li>

        //           <li><strong>Total day(s):</strong> {totalDays}</li> 
        //           <li><strong>Leave Type:</strong> {leaveName}</li> 
        //            <li><strong>Reason:</strong> {entityVM.ReasonEdit}</li>
        //        </ul>

        //        <p>Please log in to the HRM system to review and approve this request.</p>
        //         <p>
        //    <a href='' style='padding:8px 12px;background:#007bff;color:#fff;text-decoration:none;border-radius:4px;'>Login</a>
        //    &nbsp;&nbsp;
        //    <a href='' style='padding:8px 12px;background:#28a745;color:#fff;text-decoration:none;border-radius:4px;'>Accept</a>
        //    &nbsp;&nbsp;
        //    <a href='' style='padding:8px 12px;background:#dc3545;color:#fff;text-decoration:none;border-radius:4px;'>Decline</a>
        //</p>
        //        <p>Regards,<br/>HRM System</p>
        //    "
        //                };

        //                // Send using EmailService (SMTP uses applicant’s org config)
        //                await emailService.SendEmailAsync(emailModel, entityVM.EmployeeIDEdit);

        //                await leaveRequest.CommitTransactionAsync();
        //                return new CommonReturnViewModel
        //                {
        //                    Success = true,
        //                    Message = "Updated Successfully."
        //                };
        //            }
        //            catch (Exception ex)
        //            {
        //                await leaveRequest.RollbackTransactionAsync();
        //                Console.WriteLine(ex.Message);
        //                return new CommonReturnViewModel
        //                {
        //                    Success = false,
        //                    Message = "An error occurred while saving the leave request Update."
        //                };
        //            }
        //        }


        //

        #endregion


        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationApprovalModifyVM entityVM, string url)
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
                // Fetch the leave request
                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
                if (entity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Leave request not found."
                    };
                }

                // Server-side validation for date range
                if (!ValidateLeaveDates(entityVM, entity, out var errors))
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = string.Join(" ", errors)
                    };
                }

                // Get employee office info
                var offf = await empoffi.AllActive()
                    .Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
                    .Select(x => new
                    {
                        x.EmployeeID,
                        x.OrganizationID,
                        x.OrganizationBranchID,
                        x.DepartmentID,
                        x.DesignationID,
                        x.ImmediateSupervisorId,
                        x.SeniorSupervisorId,
                        x.HeadOfDepartmentId
                    })
                    .FirstOrDefaultAsync();

                if (offf == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee office info not found."
                    };
                }

                // Get approval settings
                var approvalSettings = await approvalSettingsRepository.AllActive()
                    .Include(x => x.ApprovalType)
                    .FirstOrDefaultAsync(x =>
                        (x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID) &&
                        x.ApprovalType.ApprovalTypeName == "Leave Request Approval");

                if (approvalSettings == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Your company does not exist in approver settings."
                    };
                }

                // Define approval flow
                var approvalFlow = new List<(int? id, bool isDesignation)>
        {
            (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
            (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
            (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
        };

                // Resolve approvers
                int? resolvedFirst = await ResolveApprovalAsync(approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID, offf);
                int? resolvedSecond = await ResolveApprovalAsync(approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID, offf);
                int? resolvedThird = await ResolveApprovalAsync(approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID, offf);

                // Determine current approver
                bool isFirstApprover = resolvedFirst == entityVM.UpdatedBy;
                bool isSecondApprover = resolvedSecond == entityVM.UpdatedBy;
                bool isThirdApprover = resolvedThird == entityVM.UpdatedBy;
                bool allowSelfApprover = approvalSettings.SelfExceptionApprovalID == entityVM.UpdatedBy;

                // Check if user is authorized
                if (!isFirstApprover && !isSecondApprover && !isThirdApprover && !allowSelfApprover)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "You are not authorized to approve this leave request."
                    };
                }

                // Determine approval step and finality
               // bool hasFirst = approvalSettings.FirstApprovalID.HasValue;
               // bool hasSecond = approvalSettings.SecondApprovalID.HasValue && approvalSettings.IsEnableSecondApproval;
               // bool hasThird = approvalSettings.ThirdApprovalID.HasValue && approvalSettings.IsEnableThirdApproval;

                bool isFinalApproval = false;
                int? approvalPersonId = null;
                int approvalStep = 0;

                //if (isFirstApprover)
                //{
                //    approvalStep = 1;
                //    approvalPersonId = hasSecond ? resolvedSecond : entityVM.CreatedBy;
                //    isFinalApproval = !hasSecond && !hasThird && entityVM.Approved;
                //}
                //else if (isSecondApprover)
                //{
                //    approvalStep = 2;
                //    approvalPersonId = hasThird ? resolvedThird : entityVM.CreatedBy; 
                //    isFinalApproval = !hasThird && entityVM.Approved;
                //}
                //else if (isThirdApprover)
                //{
                //    approvalStep = 3;
                //    approvalPersonId = resolvedThird; // Last in chain
                //    isFinalApproval = entityVM.Approved;
                //}
                //else if (allowSelfApprover && approvalSettings.AllowSelfApproval.HasValue && approvalSettings.AllowSelfApproval.Value)
                //{
                //    approvalStep = 4;
                //    approvalPersonId = null;
                //    isFinalApproval = entityVM.Approved;
                //}



                bool hasFirst = approvalSettings.FirstApprovalID.HasValue && resolvedFirst != null && resolvedFirst != 0;
                bool hasSecond = approvalSettings.SecondApprovalID.HasValue && approvalSettings.IsEnableSecondApproval && resolvedSecond != null && resolvedSecond != 0;
                bool hasThird = approvalSettings.ThirdApprovalID.HasValue && approvalSettings.IsEnableThirdApproval && resolvedThird != null && resolvedThird != 0;
                if (isFirstApprover)
                {
                    approvalStep = 1;
                  
                    if (!hasSecond && !hasThird)
                    {
                        approvalPersonId = entityVM.CreatedBy;
                        if (entityVM.Approved)
                        {
                            isFinalApproval = true;
                        }

                    }
                    else
                    {
                       
                        approvalPersonId = hasSecond ? resolvedSecond : (hasThird ? resolvedThird : null);
                    }
                }
                else if (isSecondApprover)
                {
                    approvalStep = 2;
                    if (!hasThird)
                    {
                        approvalPersonId = entityVM.CreatedBy;
                        if(entityVM.Approved)
                        {
                            isFinalApproval = true;
                        }
                      
                    }
                    else
                    {
                        approvalPersonId = resolvedThird;
                    }
                }
                else if (isThirdApprover)
                {
                    approvalStep = 3;
                    approvalPersonId = entityVM.CreatedBy; 
                    isFinalApproval = true; 
                }
                else if (allowSelfApprover && approvalSettings.AllowSelfApproval == true)
                {
                    approvalStep = 4;
                    approvalPersonId = null; 
                    isFinalApproval = true;
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
                        Message = "Approval or decline must be selected."
                    };
                }
                string secrectCode=string.Empty;
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

                if (isFinalApproval && entityVM.Approved)
                {
                    var leaveDaysFromConfig = await leaveTypesRepository.AllActive()
                        .Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit)
                        .Select(x => new { x.LeaveDays, x.ApplicableYear })
                        .FirstOrDefaultAsync();

                    if (leaveDaysFromConfig == null)
                    {
                        await leaveRequest.RollbackTransactionAsync();
                        return new CommonReturnViewModel
                        {
                            Success = false,
                            Message = "Leave type configuration not found."
                        };
                    }

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
                        existingBalance.UpdatedAt = DateTime.UtcNow;
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
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = entityVM.CreatedBy,
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC
                        };
                        await leaveBalance.AddAsync(newBalance);
                    }
                }

                // Update leave request with isFinalApproval
                entity.ApprovalPersonID = approvalPersonId;
                entity.StatusID = statusId;
                entity.Reason = entityVM.ReasonEdit;
                entity.LIP = entityVM.LIP;
                entity.LMAC = entityVM.LMAC;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = entityVM.UpdatedBy;
                entity.IsFinalApproved = isFinalApproval; 
                entity.ApprovalStage=approvalStep;
                entity.SecrectCode = Guid.NewGuid().ToString();
                entity.SecrectCodeDateTime = DateTime.UtcNow;
                await leaveRequest.UpdateAsync(entity);
                secrectCode=entity.SecrectCode;
                // Save approval history
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
                    ApproveBy = entityVM.UpdatedBy,
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

                // Email notification

                var allEmployeeData = await (from emp in employee.AllActive()
                                             join empOff in empoffi.AllActive().Include(x => x.Department).Include(x => x.Designation)
                                                 on emp.EmployeeID equals empOff.EmployeeID

                                             select new
                                             {
                                                 emp.EmployeeID,
                                                 emp.FirstName,
                                                 emp.LastName,
                                                 emp.Email,
                                                 empOff.OfficeEmail,
                                                 DepartmentName = empOff.Department.DepartmentName,
                                                 DesignationName = empOff.Designation.DesignationName
                                             }).ToListAsync();

                var applicantData = allEmployeeData.FirstOrDefault(x => x.EmployeeID == entityVM.EmployeeIDEdit);
               
                
               var approverData  = allEmployeeData.FirstOrDefault(x => x.EmployeeID == approvalPersonId);
                
               

                var leaveName = await leaveTypesRepository.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
               
                int totalDays = 0;

                if (entityVM.FromDateEdit.HasValue && entityVM.ToDateEdit.HasValue)
                {
                    totalDays = (entityVM.ToDateEdit.Value.DayNumber - entityVM.FromDateEdit.Value.DayNumber) + 1;
                }
                string toEmail;
                string statusMessage;
                string name=string.Empty;
                bool isApplicant = statusId == leavStatusDecline || isFinalApproval;
                if (isApplicant)
                {
                    // Applicant receives final approval/decline
                    toEmail = applicantData.OfficeEmail ?? applicantData.Email ?? string.Empty;
                    statusMessage = statusId == leavStatusDecline ? "Your leave request has been declined." : "Your leave request has been approved.";
                    name = $"{applicantData?.FirstName} {applicantData?.LastName}"; //({applicantData?.DesignationName}, {applicantData?.DepartmentName}
                }
                else
                {
                    name="HR Team";
                    toEmail = approverData?.OfficeEmail ?? approverData?.Email ?? string.Empty;
                    statusMessage = $"This is an automated leave request submitted by an employee. Please find the details below:";
                }
                var orgainfo = await _organizationRepository.AllActive().Include(x => x.Country).Include(x => x.EmployeeOfficeInfo.Where(x => x.EmployeeID == entityVM.EmployeeIDEdit))
               .Select(x => new {
                   x.OrganizationName,
                   x.LogoLink,
                   x.FaviconLink,
                   x.Address,
                   x.FullAddress,
                   x.EmailAddress,
                   CountryName = x.Country.CountryName ,
                   x.Phone
               }).FirstOrDefaultAsync();
                string formattedAddress = string.Empty;
                if (orgainfo != null && !string.IsNullOrWhiteSpace(orgainfo.Address))
                {
                    var parts = orgainfo.Address.Split(',');
                    formattedAddress = string.Join("<br>", parts.Select(p => p.Trim()));
                }
                else
                {
                    formattedAddress = "No address available"; 
                }
                string logourl;
                if (!string.IsNullOrWhiteSpace(orgainfo.LogoLink))
                {
                    logourl = $"http://usasoft.xyz/media/company/logo/{orgainfo.LogoLink}";
                }
                else
                {
                    var firstLetter = string.IsNullOrWhiteSpace(orgainfo.OrganizationName)
                        ? "?" : orgainfo.OrganizationName.Trim()[0].ToString().ToUpper();
                    logourl = $"https://ui-avatars.com/api/?name={firstLetter}&background=0D8ABC&color=fff&size=128";
                }
                if (orgainfo == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Organization Info does not exists"
                    };
                }

                //

                var model = new EmailTemplateVM
                {
                    LogoUrl = logourl,
                    FormattedAddress = formattedAddress ?? "No address available",
                    CountryName = orgainfo.CountryName ?? "No country",
                    Email = orgainfo.EmailAddress ?? "No email",
                    Phone = orgainfo.Phone ?? "No phone",
                    RecipientName = applicantData.FirstName + " " + applicantData.LastName,
                    StatusMessage = statusMessage,
                    ApplicantName = applicantData.FirstName + " " + applicantData.LastName,
                    Department = applicantData.DepartmentName,
                    Designation = applicantData.DesignationName,
                    LeaveName = leaveName,
                    FromDate = entityVM.FromDateEdit,
                    ToDate = entityVM.ToDateEdit,
                    Reason = entityVM.ReasonEdit,
                    AcceptUrl = $"{url}/LeaveApprovalDeclineRoute/Action?leaveId={entityVM.LeaveApplicationID}&approverId={approvalPersonId}&isApproved=true&secrectCode={secrectCode}",
                    DenyUrl = $"{url}/LeaveApprovalDeclineRoute/Action?leaveId={entityVM.LeaveApplicationID}&approverId={approvalPersonId}&isApproved=false&secrectCode={secrectCode}",
                    ModifyLink = $"{url}/Account/Login?returnUrl=%2FLeaveApprovalDecline%2FIndex%3FleaveApplicationID%3D{entityVM.LeaveApplicationID}",
                    IsApplicant = isApplicant
                };

                // var emailBody = await commonDroDownService.RenderViewToStringAsync("LeaveApprovalDecline/LeaveRequestEmail", model);

                //var emailModel33 = new EmailVM
                //{
                //    To = applicantData?.Email ?? applicantData?.OfficeEmail,
                //    Subject = $"Leave Application from {applicantData?.FirstName} {applicantData?.LastName}",
                //    Body = emailBody
                //};

                //
               var supervisors = await empoffi.AllActive().Where(x => x.EmployeeID == entityVM.EmployeeIDEdit)
              .Select(x => new
              {
                  ImmediateSupervisor = x.ImmediateSupervisorId != null && x.ImmediateSupervisorId != 0 ? x.ImmediateSupervisorId : (int?)null,
                  SeniorSupervisor = x.SeniorSupervisorId != null && x.SeniorSupervisorId != 0 ? x.SeniorSupervisorId : (int?)null,
                  HeadOfDepartment = x.HeadOfDepartmentId != null && x.HeadOfDepartmentId != 0 ? x.HeadOfDepartmentId : (int?)null
              }).FirstOrDefaultAsync();

                var supervisorList = new List<(string Role, int? Id)>();
                if (supervisors?.ImmediateSupervisor != null) supervisorList.Add(("Immediate Supervisor", supervisors.ImmediateSupervisor));
                if (supervisors?.SeniorSupervisor != null) supervisorList.Add(("Senior Supervisor", supervisors.SeniorSupervisor));
                if (supervisors?.HeadOfDepartment != null) supervisorList.Add(("Head of Department", supervisors.HeadOfDepartment));


                var supervisorIds = supervisorList.Select(s => s.Id).ToList();
                var supervisorNames = await employee.AllActive().Where(x => supervisorIds.Contains(x.EmployeeID))
                    .ToDictionaryAsync(x => x.EmployeeID, x => x.FirstName + " " + x.LastName);

                //
                   var approvedPerson =await GetByPersonLeaveStepVM(entityVM.LeaveApplicationID);
                    var timelineHtml = new StringBuilder();
               
                // Step 1: Submitted (always first)
                timelineHtml.Append($@"
<td style=""width:20%; position:relative; vertical-align:top;"">
  <div style=""width:50px;height:50px;background:linear-gradient(135deg, #10b981, #059669);border-radius:50%;margin:auto;display:flex;align-items:center;justify-content:center;"">
    <span style=""color:#fff;font-size:20px;font-weight:bold;margin:10px 0px 0px 17px !important;"">✓</span>
  </div>
  <p style=""margin:10px 0 5px;font-weight:bold;color:#10b981;font-size:14px;"">Submitted</p>
  <p style=""margin:0;font-size:11px;color:#6b7280;"">{DateTime.Now:MMM dd, yyyy}</p>
  <p style=""margin:0;font-size:11px;color:#6b7280;"">{DateTime.Now:hh:mm tt}</p>
</td>");

                // Connector after submitted
                if (supervisorList.Count > 0)
                {
                    timelineHtml.Append(@"<td style=""width:10%; vertical-align:middle;"">
                   <div style=""height:3px;background:#10b981;margin:0 5px;""></div>
               </td>");
                }

                // Dynamic approvals
                for (int i = 0; i < supervisorList.Count; i++)
                {
                    var sup = supervisorList[i];
                    string supName;
                    if (!supervisorNames.TryGetValue((int)sup.Id, out supName))
                    {
                        supName = "Pending";
                    }


                    // Assign colors/icons dynamically
                    //string bgColor = i == 0 ? "linear-gradient(135deg, #fbbf24, #f59e0b)" : "linear-gradient(135deg, #e5e7eb, #d1d5db)";
                    //string icon = i == 0 ? "⏳" : "•••";
                    //string textColor = i == 0 ? "#f59e0b" : "#9ca3af";
                   
                    // Default

                    var personStatus = approvedPerson.FirstOrDefault(p => p.ApprovarPerson.Contains(supName))?.StatusName ?? "Pending";

                    // Default colors/icons
                    string bgColor = "linear-gradient(135deg, #e5e7eb, #d1d5db)";
                    string icon = "•••";
                    string textColor = "#9ca3af";
                    string displayStatus = "Pending"; 
                    // Assign based on status
                    if (personStatus.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
                    {
                        bgColor = "linear-gradient(135deg, #34d399, #059669)"; // green
                        icon = "✔️";
                        textColor = "#059669";
                        displayStatus = "APPROVED"; // Add this
                    }
                    else if (personStatus.Equals("DECLINED", StringComparison.OrdinalIgnoreCase))
                    {
                        bgColor = "linear-gradient(135deg, #f87171, #dc2626)"; // red
                        icon = "❌";
                        textColor = "#dc2626";
                        displayStatus = "DECLINED"; // Add this
                    }
                    else // Pending
                    {
                        bgColor = i == 0 ? "linear-gradient(135deg, #fbbf24, #f59e0b)" : "linear-gradient(135deg, #e5e7eb, #d1d5db)";
                        icon = i == 0 ? "⏳" : "•••";
                        textColor = i == 0 ? "#f59e0b" : "#9ca3af";
                        displayStatus = "Pending"; // Add this
                    }


                    string stepText = sup.Role;

                    timelineHtml.Append($@"
                         <td style=""width:20%; position:relative; vertical-align:top;"">
                        <div style=""width:50px;height:50px;background:{bgColor};border-radius:50%;margin:auto;display:flex;align-items:center;justify-content:center;"">
                        <span style=""color:#fff;font-size:20px;font-weight:bold; margin:10px 0px 0px 17px !important;"">{icon}</span>
                      </div>
                      <p style=""margin:10px 0 5px;font-weight:bold;color:{textColor};font-size:14px;"">{stepText}</p>
                      <p style=""margin:0;font-size:11px;color:#6b7280;"">{displayStatus} </p>
                      <p style=""margin:5px 0 0;font-size:12px;color:#6b7280;font-weight:600;background:#f3f4f6;padding:3px 10px;border-radius:10px;display:inline-block;"">{supName}</p>
                      </td>");


                    //

                    // Connector after each supervisor except last
                    if (i != supervisorList.Count - 1)
                    {
                        timelineHtml.Append($@"<td style=""width:10%; vertical-align:middle;"">
                           <div style=""height:3px;background:#e5e7eb;margin:0 5px;""></div>
                             </td>");
                    }
                }
                //

                string buttonHtml = string.Empty;
                //
                if (!isApplicant)
                {
                    buttonHtml = $@"
                    <tr>
                        <td class=""content section-button"">
                            <div style=""margin-bottom: 15px; text-align: center;"">
                               <a href=""{url}/LeaveApprovalDeclineRoute/Action?leaveId={entityVM.LeaveApplicationID}&approverId={approvalPersonId}&isApproved=true&secrectCode={secrectCode}""
                                style=""display: inline-block; padding: 10px 20px; margin-right: 20px; background-color:#fff;border:1px solid #28a745;color:#28a745; text-decoration: none; border-radius: 5px; font-weight: bold;"">
                                Accept
                             </a>
                             <a href=""{url}/LeaveApprovalDeclineRoute/Action?leaveId={entityVM.LeaveApplicationID}&approverId={approvalPersonId}&isApproved=false&secrectCode={secrectCode}""
                                style=""display: inline-block; padding: 10px 20px; background-color: #fff; color:#dc3545;border:1px solid #dc3545; text-decoration: none; border-radius: 5px; font-weight: bold;"">
                                Deny
                             </a>
                            </div>
                            <p>If you need to modify applied date, kindly change by 
                               <a href=""{url}/Account/Login?returnUrl=%2FLeaveApprovalDecline%2FIndex%3FleaveApplicationID%3D{entityVM.LeaveApplicationID}"">clicking this link</a>
                            </p>
                        </td>
                    </tr>";
                }


                var emailModel = new EmailVM
                {
                    To = toEmail,
                    //To=applicantData.OfficeEmail ?? applicantData.Email,
                    Subject = $"Leave Application from {applicantData?.FirstName} {applicantData?.LastName}",
                    Body = $@"
                        <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>HR Leave Request</title>
    <style>
        /* Reset styles */
        body, table, td, p, a, li, h1, h2 {{
            -webkit-text-size-adjust: 100%;
            -ms-text-size-adjust: 100%;
            margin: 0;
            padding: 0;
        }}
        body {{
            font-family: Arial, sans-serif;
            font-size: 14px;
            line-height: 20px;
            color: #333333;
            background-color: #f4f4f4;
            padding: 20px;
        }}
        table {{
            border-collapse: collapse;
        }}

        /* Main container */
        .email-container {{
            max-width: 600px;
            margin: auto;
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            overflow: hidden;
        }}

        /* Header */
        .header-bg {{
            position: relative;
            background-color: #3252ff;
            background-image: linear-gradient(to bottom right, #080301 120px, transparent 0);
            background-repeat: no-repeat;
            background-size: 140px 140px;
            padding: 25px 30px;
            color: #ffffff;
        }}
        .header-bg img {{
            display: block;
            border: 0;
            outline: none;
            text-decoration: none;
            max-width: 200px;
            height: auto;
        }}
        .header-bg td {{
            font-size: 13px;
            line-height: 18px;
            text-align: right;
            color: #ffffff;
        }}

        /* Content */
        .content {{
            padding: 20px 30px;
        }}
        .content p {{
            margin-bottom: 10px;
        }}
        .content h2 {{
            font-size: 18px;
            margin-bottom: 10px;
            color: #3252ff;
        }}

        /* Tables for info */
        .info-table {{
            width: 100%;
            border: 1px solid #e0e0e0;
            border-radius: 5px;

        }}
        .info-table th, .info-table td {{
            padding: 10px;
            border: 1px solid #e0e0e0;
            text-align: left;

        }}
        .info-table th {{
           background-color: #f4f4f4; 
            font-weight: bold;
         			width:50%;
        }}

        /* Approval timeline */
        .timeline {{
            width: 100%;
            margin-top: 20px;
        }}
        .timeline td {{
            vertical-align: top;
        }}
        .timeline-dot {{
            width: 15px;
            height: 15px;
            border-radius: 50%;
            margin-top: 3px;
        }}
        .timeline-line {{
            width: 2px;
            height: 30px;
            background-color: #e0e0e0;
            margin-left: 6px;
        }}
      		/* Section backgrounds */
.section-header {{
    background-color: #3252ff; /* Blue header */
    color: #ffffff;
}}
.section-greeting {{
    background-color: #f9f9f9; /* light grey */
}}
.section-timeline {{
    background-color: #eef4ff; /* soft blue */
}}
.section-info {{
    background-color: #ffffff; /* white card */
}}
.section-footer {{
    background-color: #000; /* footer grey */
}}
.section-button {{
    padding-top: 0;
}}

        /* Footer */
        .footer {{
            text-align: center;
            padding: 20px 30px;
            font-size: 13px;
            color: #fff;
        }}
        /* Responsive */
        @media only screen and (max-width: 600px) {{
            .header-bg td {{
                display: block;
                text-align: center;
                margin-bottom: 10px;
            }}
            .header-bg img {{
                margin: auto;
            }}
        }}
    </style>
</head>
<body>
    <table class=""email-container"">
        <!-- Header -->
        <tr>
            <td class=""header-bg"">
                <table width=""100%"">
                    <tr>
                        <td align=""left"">
                            <img src=""{logourl}"" alt=""Company Logo"">
                        </td>
                        <td align=""right"">
                          {formattedAddress ?? "No address available"}<br>
                         {orgainfo.CountryName ?? "No country"}<br>
                        {orgainfo.EmailAddress ?? "No email"}<br>
                        {orgainfo.Phone ?? "No phone"}
                        </td>
                    </tr>
                </table>
            </td>
        </tr>

        <!-- Greeting -->
        <tr>
            <td class=""content section-greeting"">
                <p>Dear {name},</p>
                <p>{statusMessage}</p>
            </td>
        </tr>
      		<!-- Approval Timeline (Horizontal) -->
<tr>
  <td class=""content section-timeline"">
    <h2>Approval Status Timeline</h2>
    <table width=""100%"" style=""text-align:center; margin-top:20px;"">
      <tr>
        {timelineHtml}
     
      </tr>
    </table>
  </td>
</tr>


        <!-- Employee Info -->
        <tr>
            <td class=""content section-info"">
                <h2>Employee Information</h2>
                <table class=""info-table"" style=""margin-bottom: 10px;"">
                    <tr>
                        <th>Name</th>
                        <td>{applicantData.FirstName} {applicantData.LastName}</td>
                    </tr>
                    <tr>
                        <th>Department</th>
                        <td>{applicantData.DepartmentName}</td>
                    </tr>
                    <tr>
                        <th>Position Title</th>
                        <td>{applicantData.DesignationName}</td>
                    </tr>
                </table>

                <h2>Leave Details</h2>
                <table class=""info-table"">
                    <tr>
                        <th>Type of Leave</th>
                        <td>{leaveName}</td>
                    </tr>
                    <tr>
                        <th>Start Date</th>
                        <td>{entityVM.FromDateEdit:dd MMM yyyy}</td>
                    </tr>
                    <tr>
                        <th>End Date</th>
                        <td>{entityVM.ToDateEdit:dd MMM yyyy}</td>
                    </tr>
                    <tr>
                        <th>Reason</th>
                        <td>{entityVM.ReasonEdit}</td>
                    </tr>
                </table>
            </td>
        </tr>





<!-- Button Info -->
<tr>
      
  
{buttonHtml}
</tr>
<!-- Footer -->
<tr>
  <td class=""footer section-footer"" align=""center"" style=""text-align:center;"">
    <p>© Gctlinfosys 2025. All rights reserved.</p>


  </td>
</tr>



    </table>
</body>
</html>
                        "
                };

                //

                await emailService.SendEmailLeaveRequest(emailModel, entityVM.EmployeeIDEdit);
                await leaveRequest.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Updated successfully."
                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        //

       

        private async Task<List<PersonLeaveStepVM>> GetByPersonLeaveStepVM(int leaveApplicationID)
        {
            int? TotalSupCount = 0;
            var empId = await leaveRequest.AllActive()
                .Where(x => x.LeaveApplicationID == leaveApplicationID)
                .Select(x => x.EmployeeID)
                .FirstOrDefaultAsync();

            var empInfo = await empoffi.AllActive()
                .Where(x => x.EmployeeID == empId)
                .Select(x => new
                {
                    Imm = x.ImmediateSupervisorId != null ? 1 : 0,
                    Sen = x.SeniorSupervisorId != null ? 1 : 0,
                    HOd = x.HeadOfDepartmentId != null ? 1 : 0,
                })
                .FirstOrDefaultAsync();

            TotalSupCount = (empInfo?.Imm ?? 0) + (empInfo?.Sen ?? 0) + (empInfo?.HOd ?? 0);

            var result = await (
                from lb in leaveBaseAprovalHistory.AllActive()
                    .Where(x => x.LeaveApplicationID == leaveApplicationID)
                    .AsNoTracking()
                join statusName in leaveStatuses.AllActive()
                    .Select(x => new { x.StatusID, x.StatusName })
                    on lb.StatusID equals statusName.StatusID
                join e in employee.AllActive()
                    .Select(x => new { x.EmployeeID, x.FirstName, x.LastName })
                    on lb.ApproveBy equals e.EmployeeID
                join leaveReq in leaveRequest.AllActive()
                    on lb.LeaveApplicationID equals leaveReq.LeaveApplicationID
                select new PersonLeaveStepVM
                {
                    ApprovarNote = lb.ApproverNote ?? string.Empty,
                    ApproverStep = lb.ApprovalStep ?? 0,
                    ApprovarPerson = e.FirstName + " " + e.LastName ?? string.Empty,
                    StatusName = statusName.StatusName ?? string.Empty,
                    ApproverStepTotal = TotalSupCount,
                    ApprovedOrDeclineDate = lb.CreatedAt.HasValue ? TimeConversionHelper.ConvertUtcToUserLocalizedDateTimeString(DateTime.SpecifyKind(lb.CreatedAt.Value, DateTimeKind.Utc), _localizationContext) : "-",
                }).OrderBy(x => x.ApproverStep).ToListAsync();

            return result ?? new List<PersonLeaveStepVM>();
        }

        #endregion

        #region Dispaly LeaveDays 
        public async Task<List<LeaveBalancesDisplayVM>> GetLeaveTypeBalancesForEmployee(string userId)
        {
            var workingHour = await leavePolicyConfiguration.AllActive().Select(x => x.WorkingHour).FirstOrDefaultAsync();

            if (workingHour == null || workingHour == 0) workingHour = 8;
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
                                    ? ((lb.TotalLeave ?? 0) - ((lb.Taken ?? 0) + (lb.TakenPartialHours/workingHour ?? 0)))
                                    : (lt.LeaveDays ?? 0)
                            };
           return await baseQuery.ToListAsync();



            #endregion


        }
    }
    }
