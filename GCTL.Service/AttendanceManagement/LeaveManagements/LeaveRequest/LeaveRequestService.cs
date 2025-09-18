using Azure.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveRequest
{
    public class LeaveRequestService:AppService<LeaveApplications>, ILeaveRequestService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly IGenericRepository<LeaveTypes> leaveTypes; 
        private readonly IGenericRepository<Statuses> leaveStatuses;
        private readonly IUserInfoService userInfoService;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<Holidays> holidays;
        private readonly IGenericRepository<WeekendSettings> weenkendsettings;
        private readonly IGenericRepository<WeekendDays> weekedays;
        private readonly IGenericRepository<LeaveBalances> leaveBalances;
        private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        private readonly IGenericRepository<LeaveBaseApprovalHistory> leaveBaseApprovalHistory;
        private readonly IEmailService  emailService;
        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest, IGenericRepository<LeaveTypes> leaveTypes, IGenericRepository<Statuses> leaveStatuses, IUserInfoService userInfoService, IGenericRepository<Data.Models.Employees> employee, AppDbContext appDb, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<Holidays> holidays, IGenericRepository<WeekendSettings> weenkendsettings, IGenericRepository<WeekendDays> weekedays, IGenericRepository<LeaveBalances> leaveBalances, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository, IGenericRepository<ApprovalDesignation> approvaldesignation, IGenericRepository<LeaveBaseApprovalHistory> leaveBaseApprovalHistory, IEmailService emailService) : base(leaveRequest)
        {
            this.leaveRequest = leaveRequest;
            this.leaveTypes = leaveTypes;
            this.leaveStatuses = leaveStatuses;
            this.userInfoService = userInfoService;
            this.employee = employee;
            this.appDb = appDb;
            this.leavePolicyConfiguration = leavePolicyConfiguration;
            this.empoffi = empoffi;
            this.holidays = holidays;
            this.weenkendsettings = weenkendsettings;
            this.weekedays = weekedays;
            this.leaveBalances = leaveBalances;
            _organizationRepository = organizationRepository;
            _departmentRepository = departmentRepository;
            this.approvalSettingsRepository = approvalSettingsRepository;
            this.approvalTypesRepository = approvalTypesRepository;
            this.approvaldesignation = approvaldesignation;
            this.leaveBaseApprovalHistory = leaveBaseApprovalHistory;
            this.emailService = emailService;
        }

        #region  Get Data All  Leave  Requyest
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
          List<int> departmentIds = null, List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId select role.Name).FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = leaveRequest.AllActive().Include(x => x.LeaveBaseApprovalHistory).Include(x => x.Employee).Include(x => x.Status).Include(x => x.LeaveType).OrderByDescending(x => x.LeaveApplicationID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
                }
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


                // 🔹 Step 4: Filter if not SuperAdmin
                if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.EmployeeID == employeeId);
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
                Expression<Func<LeaveApplications, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "employeename" => x => x.Employee.FirstName + " " + x.Employee.LastName,
                    "leavetype" => x => x.LeaveType.LeaveTypeName,
                    "fromdate" => x => x.FromDate,
                    "todate" => x => x.ToDate,
                    "period" => x => x.ToDate.DayNumber - x.FromDate.DayNumber + 1,
                    "statusname" => x => x.Status.StatusName,
                    "applyDate" => x => x.CreatedAt,
                    _ => x => x.LeaveApplicationID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

                //
                //var data = await query.ToListAsync(); // Load all into memory

                // Apply Period calculation manually for sorting
                //if (currentSortColumn?.ToLower() == "period")
                //{
                //    data = currentSortOrder == "desc"
                //        ? data.OrderByDescending(b =>
                //            b.IsFullDay
                //                ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1
                //                : b.PartialFromTime.HasValue && b.PartialToTime.HasValue
                //                    ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours
                //                    : 0).ToList()
                //        : data.OrderBy(b =>
                //            b.IsFullDay
                //                ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1
                //                : b.PartialFromTime.HasValue && b.PartialToTime.HasValue
                //                    ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours
                //                    : 0).ToList();
                //}

                // For approver Step
                var approvalStepsMap = await leaveBaseApprovalHistory.AllActive().GroupBy(x => x.LeaveApplicationID).ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ApprovalStep ?? 0).ToList());
                //
                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                   term => b =>
                      EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%") ||
                      EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%") ||
                      EF.Functions.Like(b.LeaveType.LeaveTypeName, $"%{term}%") ||
                      EF.Functions.Like(b.Status.StatusName, $"%{term}%") ||
                      EF.Functions.Like(b.FromDate.ToString(), $"%{term}%") ||
                      EF.Functions.Like(b.ToDate.ToString(), $"%{term}%"),


                    b => new LeaveApplicationsList
                    {
                        ApplicationDateForTable = DateTimeHelpers.FormatDateTime(b.CreatedAt)  ,
                        ApplicationDate = b.CreatedAt,
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
                        ApproverStep = approvalStepsMap.ContainsKey(b.LeaveApplicationID) ? approvalStepsMap[b.LeaveApplicationID] : new List<int>()

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
        public async Task<LeaveApplicationEditVM> GetLeaveRequestByIdAsync(int leaveApplicationID)
        {
            if (leaveApplicationID ==0) 
            {
                return null;
            }

            await leaveRequest.BeginTransactionAsync();
            try
            {
                var data = await leaveRequest.GetByIdAsync(leaveApplicationID);
                if (data == null) return null;

                //
                var leaveBalance = await leaveBalances.AllActive()
                          .Where(x => x.EmployeeID == data.EmployeeID && x.LeaveTypeID == data.LeaveTypeID)
                          .Select(x => new
                          {
                              leaveDays = x.TotalLeave - x.Taken
                          }).FirstOrDefaultAsync();

                decimal availableDays = 0;

                if (leaveBalance != null)
                {
                    availableDays = leaveBalance.leaveDays ?? 0;
                }
                else
                {
                    var defaultLeave = await leaveTypes.AllActive()
                        .Where(l => l.LeaveTypeID == data.LeaveTypeID)
                        .Select(l => new
                        {
                            leaveDays = l.LeaveDays
                        }).FirstOrDefaultAsync();

                    if (defaultLeave == null) return null;

                    availableDays = defaultLeave.leaveDays ?? 0;
                }
                //
                SubsequentVM? subsequent = null;

                var fromDateTime = data.FromDate.ToDateTime(TimeOnly.MinValue);
                var toDateTime = data.ToDate.ToDateTime(TimeOnly.MinValue);

                subsequent = await SubsequentAsynce(fromDateTime, toDateTime);

                LeaveApplicationEditVM entityVM = new LeaveApplicationEditVM
                { 
                LeaveApplicationID = data.LeaveApplicationID,
                EmployeeIDEdit=data.EmployeeID,
                LeaveTypeIDEdit=data.LeaveTypeID,
                ReasonEdit=data.Reason,
                IsFullDayEdit=data.IsFullDay,
                FromDateEdit=data.FromDate,
                ToDateEdit=data.ToDate,
                PartialFromTimeEdit=data.PartialFromTime,
                PartialToTimeEdit=data.PartialToTime,
                LeaveDaysEdit = availableDays,
                Period = data.IsFullDay ? (data.ToDate.DayNumber - data.FromDate.DayNumber) + 1 : data.PartialFromTime.HasValue && data.PartialToTime.HasValue ? (int)(data.PartialToTime.Value - data.PartialFromTime.Value).TotalHours : 0,
                TotalSubsequentDays = subsequent?.TotalSubsequentDays,
               IsHolidayCountedAsLeave = subsequent?.IsHolidayCountedAsLeave ?? false,
               IsWeekendCountedAsLeave = subsequent?.IsWeekendCountedAsLeave ?? false,
              
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

        #region  Save Leave Reqest

        //Today Taskk  

        // StatusID according to Name 
        private async Task<int?> GetIdByNameAsync(string name)
        {
            var data = await leaveStatuses.AllActive().Where(x => EF.Functions.Like(x.StatusName.ToLower(), name.ToLower())).Select(x => (int?)x.StatusID).FirstOrDefaultAsync();
            return data;
        }
        //LeaveTypeID according to The Name 
        private async Task<int?> GetLeaveTypeIdByNameAsync(string name)
        {
            return await leaveTypes.AllActive()
                .Where(x => EF.Functions.Like(x.LeaveTypeName.ToLower(), name.ToLower())).Select(x => (int?)x.LeaveTypeID) .FirstOrDefaultAsync();
        }
        // Calculate Hour 
        public static decimal CalculatePartialHours(TimeOnly? from, TimeOnly? to)
        {
            if (!from.HasValue || !to.HasValue)
                return 0;

            var duration = to.Value.ToTimeSpan() - from.Value.ToTimeSpan();

            if (duration.TotalMinutes <= 0)
                return 0;
            var result = Math.Round((decimal)duration.TotalMinutes / 60, 2); 
            return result;
        }
        // Self approver 
        private async Task HandleSelfApprovalAsync(LeaveApplicationsRequestVM entityVM, int? approvalPersonId, ApprovalSettings approvalSettings, int sequence, dynamic offf,
         List<(int? id, bool isDesignation)> approvalFlow)
        {
   
            int selfStep = 0;
            for (int i = 0; i < approvalFlow.Count; i++)
            {
                var (id, isDesignation) = approvalFlow[i];
                var resolvedId = await ResolveApprovalAsync(id, isDesignation, offf);
                if (resolvedId == entityVM.CreatedBy)
                {
                    selfStep = i + 1; 
                    break;
                }
            }
            //
            var fromDate = entityVM.FromDate ?? DateOnly.FromDateTime(DateTime.Today);
            var toDate = entityVM.ToDate ?? DateOnly.FromDateTime(DateTime.Today);
            int TotalAppliedDays = (toDate.DayNumber - fromDate.DayNumber) + 1;
            if (approvalSettings.AllowSelfApproval != true || approvalPersonId != entityVM.EmployeeID) return;

            int? approvedStatusId = await GetIdByNameAsync("APPROVED");
            if (approvedStatusId == null) return;

            var leaveDaysFromConfig = await leaveTypes.AllActive()
                .Where(x => x.LeaveTypeID == entityVM.LeaveTypeID)
                .Select(x => new { x.LeaveDays, x.ApplicableYear }).FirstOrDefaultAsync();

            if (leaveDaysFromConfig == null) return;

            // Update or create leave balance
            var existingBalance = await leaveBalances.AllActive()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeID == entityVM.EmployeeID &&
                    x.LeaveTypeID == entityVM.LeaveTypeID);

            if (existingBalance != null)
            {
                
                if (entityVM.IsFullDay)
                {
                    existingBalance.Taken = (existingBalance.Taken ?? 0) + TotalAppliedDays;
                }
                else
                {
                    var newPartial = CalculatePartialHours(entityVM.PartialFromTime, entityVM.PartialToTime);
                    existingBalance.TakenPartialHours = (existingBalance.TakenPartialHours ?? 0) + newPartial;
                }
                existingBalance.TotalLeave = leaveDaysFromConfig.LeaveDays;
                existingBalance.ApplicableYear = leaveDaysFromConfig.ApplicableYear;
                existingBalance.LIP = entityVM.LIP;
                existingBalance.LMAC = entityVM.LMAC;
                existingBalance.UpdatedAt = DateTime.Now;
                existingBalance.UpdatedBy = entityVM.UpdatedBy;

                await leaveBalances.UpdateAsync(existingBalance);
            }
            else
            {
                var newBalance = new LeaveBalances
                {
                    EmployeeID = entityVM.EmployeeID,
                    LeaveTypeID = entityVM.LeaveTypeID,
                    Taken = entityVM.IsFullDay ? TotalAppliedDays : 0,
                    TakenPartialHours = entityVM.IsFullDay ? 0 : CalculatePartialHours(entityVM.PartialFromTime, entityVM.PartialToTime),
                    TotalLeave = leaveDaysFromConfig.LeaveDays,
                    ApplicableYear = leaveDaysFromConfig.ApplicableYear,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };
                await leaveBalances.AddAsync(newBalance);
            }

            // Add approval history
            var leaveBase = new LeaveBaseApprovalHistory
            {
                LeaveApplicationID = sequence,  
                StatusID = approvedStatusId.Value,
                ApproverNote = "SELF APPROVED",
                LeaveTypeID = entityVM.LeaveTypeID,
                CreatedAt = DateTime.Now,
                CreatedBy = entityVM.CreatedBy,
                LIP = entityVM.LIP,
                LMAC = entityVM.LMAC,
                ApproveBy = entityVM.CreatedBy,
                ApprovalStep = selfStep
            };

            if (entityVM.IsFullDay)
            {
                leaveBase.FromDate = entityVM.FromDate ?? default;
                leaveBase.ToDate = entityVM.ToDate ?? default;
            }
            else if (entityVM.ToDateFromDateCombined.HasValue)
            {
                
                var dateOnly = entityVM.ToDateFromDateCombined.Value;
                leaveBase.FromDate = dateOnly;
                leaveBase.ToDate = dateOnly;
                leaveBase.PartialFromTime = entityVM.PartialFromTime;
                leaveBase.PartialToTime = entityVM.PartialToTime;
            }

            await leaveBaseApprovalHistory.AddAsync(leaveBase);
        }

        private async Task<bool> HasOverlappingLeave(int? employeeId, DateOnly? from, DateOnly? to, int? applicableYear)
        {
            var rejectedStatusId = await GetIdByNameAsync("DECLINED");
            return await leaveRequest.AllActive().AnyAsync(x =>
                x.EmployeeID == employeeId &&
                (rejectedStatusId == null || x.StatusID != rejectedStatusId) && 
                x.LeaveApplicableYear == applicableYear &&
                from.HasValue && to.HasValue &&
                (
                    (from.Value >= x.FromDate && from.Value <= x.ToDate) ||
                    (to.Value >= x.FromDate && to.Value <= x.ToDate) ||
                    (from.Value <= x.FromDate && to.Value >= x.ToDate)
                )
            );
        }

        // OverFlow Balances 
        private async Task<(bool AllowFallback, bool AllowLWP)> GetLeaveAdjustmentPolicyAsync()
        {
            var config = await leavePolicyConfiguration.AllActive()
                .Select(x => new
                {
                    x.IsAllowCrossLeave,
                    x.IsExceedLeaveBalance,
                    x.EnableLeaveBalanceResetDate,
                    ResetYear = x.LeaveBalanceResetDate.HasValue ? x.LeaveBalanceResetDate.Value.Year : (int?)null
                }).FirstOrDefaultAsync();

            if (config == null) return (false, false);

            if (!config.IsAllowCrossLeave && !config.IsExceedLeaveBalance) return (false, false);
            if (!config.IsAllowCrossLeave && config.IsExceedLeaveBalance) return (false, true);

            return (true, true);
        }
        // Approver persons according to Designation
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

        //According to Designationassign 
        private async Task<int?> ResolveApprovalAsync(int? approvalId, bool isDesignation, dynamic offf, int? employeeId, bool? allowSelfApproval, int? selfExceptionApprovalId)
        {
            if (!approvalId.HasValue) return null;

            int? resolvedApproverId;

            if (isDesignation)
            {
                var code = await approvaldesignation.AllActive()
                    .Where(x => x.ApprovalDesignationID == approvalId)
                    .Select(x => x.Code)
                    .FirstOrDefaultAsync();

                resolvedApproverId = code switch
                {
                    1 => offf.ImmediateSupervisorId,
                    2 => offf.SeniorSupervisorId,
                    3 => offf.HeadOfDepartmentId,
                    _ => null
                };
            }
            else
            {
                resolvedApproverId = approvalId;
            }

            // Check if self-approval is blocked
            if (resolvedApproverId.HasValue && IsSelfApprovalBlocked(employeeId, resolvedApproverId, allowSelfApproval, selfExceptionApprovalId))
            {
                return selfExceptionApprovalId; 
            }

            return resolvedApproverId;
        }

        private bool IsSelfApprovalBlocked(int? employeeId, int? approverId, bool? allowSelfApproval, int? exceptionId)
        {
            if (approverId != employeeId) return false;
            if (allowSelfApproval == true && exceptionId != employeeId) return false;
            return true;
        }
        // Vaidation For Maximum Per day according to LeavePolicyConfiguration
        private async Task<CommonReturnViewModel> ValidationMaxPerDayPartialDayAsync( int? employeeID,DateOnly? combinedDate )
        {
            // Step 1: Get max allowed partial leaves per day from policy
            var maxPartialLeavesPerDay = await leavePolicyConfiguration.AllActive().Select(x => x.ShortLeaveMaxInADay) .FirstOrDefaultAsync();

            if (maxPartialLeavesPerDay == null || maxPartialLeavesPerDay <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Short Leave Per Day policy is not configured. Please contact HR or Administrator."
                };
            }
                var partialLeavesCount = await leaveRequest.AllActive().Include(x=>x.Status).CountAsync(lr =>lr.EmployeeID == employeeID && lr.FromDate == combinedDate && lr.ToDate==combinedDate &&!lr.IsFullDay && lr.Status.StatusName== "APPROVED");

                if (partialLeavesCount >= maxPartialLeavesPerDay)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = $"Policy restricts to a maximum of {maxPartialLeavesPerDay} partial leave(s) per day.But You've already reached the limit for {combinedDate:dd/MM/yyyy}."
                    };
                }
           

            return new CommonReturnViewModel
            {
                Success = true,
              
            };
        }


        //Save Code
            public async Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM)
        {

            if (entityVM == null)
                return new CommonReturnViewModel { Success = false, Message = "Data cannot be null" };

            // 3. If it's a partial day leave, check how many partials already exist
            if (!entityVM.IsFullDay)
            {
                var result = await ValidationMaxPerDayPartialDayAsync(entityVM.EmployeeID, entityVM.ToDateFromDateCombined);
                if (!result.Success)
                {
                    return result;
                }
            }
            //
            int? applicableYear = DateTime.Now.Year;
            if (await HasOverlappingLeave(entityVM.EmployeeID, entityVM.FromDate, entityVM.ToDate, applicableYear))
                return new CommonReturnViewModel { Success = false, Message = "You already have leave on selected dates" };

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

            var approvalSettings = await approvalSettingsRepository.AllActive()
                .Include(x => x.ApprovalType).Where(x =>
                    x.OrganizationID == offf.OrganizationID &&
                    (x.OrganizationBranchID == null || x.OrganizationBranchID == offf.OrganizationBranchID) &&
                    x.ApprovalType.ApprovalTypeName == "Leave Request Approval").FirstOrDefaultAsync();

            if (approvalSettings == null) return new CommonReturnViewModel { Success = false, Message = "No active Leave Request Approval settings found." };

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
                if (resolvedId == entityVM.CreatedBy)
                {
                    isSelfApprover = true;
                    break;
                }
            }

            
            // Step 2: Self-approval logic for known fixed levels
            if (isSelfApprover)
            {
                if (approvalSettings.AllowSelfApproval==true)
                {
                    if (entityVM.CreatedBy == approvalSettings.FirstApprovalID && entityVM.EmployeeID == approvalSettings.FirstApprovalID)
                    {
                        approvalPersonId = approvalSettings.SecondApprovalID;
                    }
                    else if (entityVM.CreatedBy == approvalSettings.SecondApprovalID && entityVM.EmployeeID == approvalSettings.SecondApprovalID)
                    {
                        approvalPersonId = approvalSettings.ThirdApprovalID;
                    }
                    else if (entityVM.CreatedBy == approvalSettings.ThirdApprovalID && entityVM.EmployeeID == approvalSettings.ThirdApprovalID)
                    {
                        approvalPersonId = approvalSettings.ThirdApprovalID;
                    }
                }
                else // Self-approval not allowed
                {
                    approvalPersonId = approvalSettings.SelfExceptionApprovalID;
                }

                // Process if we got an approver in self-approval flow
                if (approvalPersonId.HasValue)
                {
                    return await ProcessLeaveApplicationAsync(entityVM, approvalPersonId.Value, approvalSettings, offf, approvalFlow, true);
                }
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

                // Skip if blocked
                if (resolvedId.HasValue &&
                    !IsSelfApprovalBlocked(entityVM.EmployeeID, resolvedId.Value,
                        approvalSettings.AllowSelfApproval, approvalSettings.SelfExceptionApprovalID))
                {
                    approvalPersonId = resolvedId.Value;
                    break;
                }
            }

            // Final step: Process leave
            if (approvalPersonId == null)
            {
                return new CommonReturnViewModel { Success = false, Message = "No valid approver found." };
            }
            return await ProcessLeaveApplicationAsync(entityVM, approvalPersonId.Value, approvalSettings, offf, approvalFlow, false);

        }




        // Max Perday Validation Partial

        //
        private async Task<CommonReturnViewModel> ProcessLeaveApplicationAsync( LeaveApplicationsRequestVM entityVM,int approvalPersonId,ApprovalSettings approvalSettings,dynamic offf,List<(int? id, bool isDesignation)> approvalFlow,bool isSelfApproval
        )
        {
            await leaveRequest.BeginTransactionAsync();
            try
            {
                var (allowFallback, allowLWP) = await GetLeaveAdjustmentPolicyAsync();
                var lWP = await GetLeaveTypeIdByNameAsync("LWP");       
                var annualLeaveType = await GetLeaveTypeIdByNameAsync("Annual Leave");   
                var fromDate = entityVM.FromDate ?? DateOnly.FromDateTime(DateTime.Today);
                var toDate = entityVM.ToDate ?? DateOnly.FromDateTime(DateTime.Today);
                int totalRequestedDays = (toDate.DayNumber - fromDate.DayNumber) + 1;
                var leaveInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeID, entityVM.LeaveTypeID);
                decimal availableDays = leaveInfo?.LeaveDays ?? 0;
                int usedDays = (int)Math.Min(totalRequestedDays, availableDays);
                int remainingDays = totalRequestedDays - usedDays;
                var leaveTypeName = await leaveTypes.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeID).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
                var strictLeaveTypes = new List<string> { "Maternity Leave", "Paternity Leave" };
                var leaveTypeIDApproved = await GetIdByNameAsync("APPROVED");
                if (remainingDays > 0 && strictLeaveTypes.Contains(leaveTypeName))
                return new CommonReturnViewModel { Success = false, Message = $"{leaveTypeName} cannot be adjusted from other leave types." };
                int sequence = 0;
                if (usedDays > 0)
                {
                    var entity = new LeaveApplications
                    {
                        EmployeeID = entityVM.EmployeeID,
                        IsFullDay = entityVM.IsFullDay,
                        FromDate = fromDate,
                        ToDate = fromDate.AddDays(usedDays - 1),
                        PartialFromTime = entityVM.PartialFromTime,
                        PartialToTime = entityVM.PartialToTime,
                        StatusID = isSelfApproval ? leaveTypeIDApproved : entityVM.StatusID,
                        LeaveApplicableYear = DateTime.Now.Year,
                        CreatedAt = DateTime.Now,
                        CreatedBy = entityVM.CreatedBy,
                        LeaveTypeID = entityVM.LeaveTypeID,
                        IsGroupApplication = entityVM.IsGroupApplication,
                        Reason = entityVM.Reason,
                        ApprovalPersonID = approvalPersonId,
                        IsFinalApproved = isSelfApproval ? true : false,
                        LIP = entityVM.LIP,
                        LMAC = entityVM.LMAC
                    };

                    await leaveRequest.AddAsync(entity);
                    sequence = entity.LeaveApplicationID;

                    await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
                }

                if (isSelfApproval)
                {
                    await HandleSelfApprovalAsync(entityVM, approvalPersonId, approvalSettings, sequence, offf, approvalFlow);
                }

                var currentStart = fromDate.AddDays(usedDays);
                if (remainingDays > 0 && allowFallback)
                {
                    var fallbackTypes = await leaveTypes.AllActive()
                        .Where(x => x.IsActive && x.LeavePriorityId != null && x.LeaveTypeID != entityVM.LeaveTypeID && x.LeaveTypeID != annualLeaveType && x.LeaveTypeID != lWP)
                        .OrderBy(x => x.LeavePriorityId).Select(x => new { x.LeaveTypeID, x.LeaveTypeName }).ToListAsync();

                    foreach (var fallback in fallbackTypes)
                    {
                        if (remainingDays <= 0) break;

                        var fallbackInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeID, fallback.LeaveTypeID);
                        int usedFallback = (int)Math.Min(remainingDays, fallbackInfo?.LeaveDays ?? 0);

                        if (usedFallback > 0)
                        {
                            var partial = new LeaveApplications
                            {
                                EmployeeID = entityVM.EmployeeID,
                                IsFullDay = entityVM.IsFullDay,
                                FromDate = currentStart,
                                ToDate = currentStart.AddDays(usedFallback - 1),
                                PartialFromTime = entityVM.PartialFromTime,
                                PartialToTime = entityVM.PartialToTime,
                                StatusID = entityVM.StatusID,
                                LeaveApplicableYear = DateTime.Now.Year,
                                IsFinalApproved = isSelfApproval ? true : false,
                                //IsGroupApplication = entityVM.IsGroupApplication,
                                CreatedAt = DateTime.Now,
                                CreatedBy = entityVM.CreatedBy,
                                LeaveTypeID = fallback.LeaveTypeID,
                                Reason = $"Exceeded original leave – adjusted using prioritized leave type ({fallback.LeaveTypeName})",
                                LIP = entityVM.LIP,
                                LMAC = entityVM.LMAC,
                                GroupApplicationID = sequence > 0 ? sequence : 0,
                                ApprovalPersonID = approvalPersonId
                            };

                            await leaveRequest.AddAsync(partial);
                            sequence = partial.LeaveApplicationID;
                            await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, partial, partial.LeaveApplicationID, entityVM);

                            remainingDays -= usedFallback;
                            currentStart = currentStart.AddDays(usedFallback);
                        }
                    }
                }

                if (remainingDays > 0 && allowLWP)
                {
                    var lwpName = await leaveTypes.AllActive().Where(x => x.LeaveTypeID == lWP).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();

                    var lwpEntity = new LeaveApplications
                    {
                        EmployeeID = entityVM.EmployeeID,
                        IsFullDay = entityVM.IsFullDay,
                        FromDate = currentStart,
                        ToDate = currentStart.AddDays(remainingDays - 1),
                        PartialFromTime = entityVM.PartialFromTime,
                        PartialToTime = entityVM.PartialToTime,
                        StatusID = entityVM.StatusID,
                        IsFinalApproved = isSelfApproval ? true : false,
                        LeaveApplicableYear = DateTime.Now.Year,
                        CreatedAt = DateTime.Now,
                        CreatedBy = entityVM.CreatedBy,
                        //IsGroupApplication= entityVM.IsGroupApplication,
                        LeaveTypeID = lWP,
                        Reason = $"Exceeded leave days – fallback to LWP ({lwpName})",
                        LIP = entityVM.LIP,
                        LMAC = entityVM.LMAC,
                        GroupApplicationID = sequence > 0 ? sequence : 0,
                        ApprovalPersonID = approvalPersonId
                    };

                    await leaveRequest.AddAsync(lwpEntity);
                    sequence=lwpEntity.LeaveApplicationID;
                    await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, lwpEntity, lwpEntity.LeaveApplicationID, entityVM);
                }
                // for email 
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

                var applicantData = allEmployeeData.FirstOrDefault(x => x.EmployeeID == entityVM.EmployeeID);
                var approverData = allEmployeeData.FirstOrDefault(x => x.EmployeeID == approvalPersonId);

                var leaveName = await leaveTypes.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeID).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
                int totalDays = 0;
                if (entityVM.FromDate.HasValue && entityVM.ToDate.HasValue)
                {
                    totalDays = (entityVM.ToDate.Value.DayNumber - entityVM.FromDate.Value.DayNumber) + 1;
                }

                // Build email model
                int leaveApplicationID = sequence;
                var emailModel = new EmailVM
                {
                    To = approverData?.Email ?? approverData?.OfficeEmail,
                    
                    Subject = $"Leave Application from {applicantData?.FirstName} {applicantData?.LastName}",
                    Body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; margin: -20px -20px 20px -20px;'>
                <h2 style='margin: 0; font-size: 24px;'>Leave Application Request</h2>
            </div>
            
            <p style='color: #333; font-size: 16px;'>Dear {approverData?.FirstName} {approverData?.LastName},</p>
            
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                <strong>{applicantData?.FirstName} {applicantData?.LastName}</strong> 
                ({applicantData?.DesignationName}, {applicantData?.DepartmentName}) has applied for leave.
            </p>
            
            <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                <h3 style='color: #667eea; margin-top: 0;'>Leave Details:</h3>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; color: #555;'>From Date:</td>
                        <td style='padding: 8px 0; color: #333;'>{entityVM.FromDate:dd MMM yyyy}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; color: #555;'>To Date:</td>
                        <td style='padding: 8px 0; color: #333;'>{entityVM.ToDate:dd MMM yyyy}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; color: #555;'>Total Days:</td>
                        <td style='padding: 8px 0; color: #333;'>{totalDays}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; color: #555;'>Leave Type:</td>
                        <td style='padding: 8px 0; color: #333;'>{leaveName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; color: #555; vertical-align: top;'>Reason:</td>
                        <td style='padding: 8px 0; color: #333;'>{entityVM.Reason}</td>
                    </tr>
                </table>
            </div>
            
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                Please log in to the HRM system to review and approve this request.
            </p>
            
            <div style='text-align: center; margin: 30px 0;'>
 <a href=""https://localhost:7086/Account/Login?returnUrl=%2FLeaveApprovalDecline%2FIndex%3FleaveApplicationID%3D{leaveApplicationID}""
               style=""display: inline-block; padding: 12px 24px; background: #007bff; color: #fff; text-decoration: none; border-radius: 6px;"">
               🔐 LOGIN TO HRM
            </a>


                <br/><br/>
<!--
           <a href='https://localhost:7086/LeaveApprovalDecline/Index?leaveApplicationID={leaveApplicationID}' 
          style='display: inline-block; padding: 12px 24px; background: #6f42c1; color: #fff; text-decoration: none; border-radius: 6px; margin: 0 5px; font-weight: bold; text-transform: uppercase; letter-spacing: 0.5px;'>
           📋 Approvals
        </a>
-->
            </div>
            
            <div style='background: #e9ecef; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                <p style='margin: 0; color: #6c757d; font-size: 14px; text-align: center;'>
                    <strong>Quick Actions:</strong> After logging in, you can directly access the leave approval page using the link above.
                </p>
            </div>
            
            <div style='border-top: 1px solid #ddd; padding-top: 20px; margin-top: 30px;'>
                <p style='color: #333; font-size: 16px; margin-bottom: 5px;'>Best Regards,</p>
                <p style='color: #667eea; font-weight: bold; font-size: 16px; margin: 0;'>HRM System</p>
                <p style='color: #6c757d; font-size: 14px; margin: 5px 0 0 0;'>Human Resource Management</p>
            </div>
        </div>
    "
                };

                // Send using EmailService (SMTP uses applicant’s org config)
                await emailService.SendEmailAsync(emailModel, entityVM.EmployeeID);

                //


                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = isSelfApproval ? "Self Approved and Saved Successfully." : "Saved Successfully."

                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = isSelfApproval ? "Self-approval failed." : "Leave request failed.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        //

        #endregion

        #region  Update Method
     
        //
        private async Task<bool> HasOverlappingLeaveUpdated(int? employeeId, DateOnly? from, DateOnly? to, int? applicableYear,int leaveApplicationID)
        {
            var rejectedStatuses = await leaveStatuses.AllActive()
                .Where(x => x.StatusName == "DECLINED")
                .Select(x => x.StatusID)
                .ToListAsync();

            return await leaveRequest.AllActive().AnyAsync(x =>
                x.EmployeeID == employeeId && x.LeaveApplicationID== leaveApplicationID &&
                !rejectedStatuses.Contains((int)x.StatusID) &&
                x.LeaveApplicableYear == applicableYear &&
                (
                    (from >= x.FromDate && from <= x.ToDate) ||
                    (to >= x.FromDate && to <= x.ToDate) ||
                    (from <= x.FromDate && to >= x.ToDate)
                )
            );
        }
        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationEditVM entityVM)
        {
            if (entityVM == null)
                return new CommonReturnViewModel { Success = false, Message = "Data cannot be null" };

            var existing = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);

            if (existing == null)
                return new CommonReturnViewModel { Success = false, Message = "Leave application not found" };

            int? applicableYear = DateTime.Now.Year;

            if (await HasOverlappingLeaveUpdated(entityVM.EmployeeIDEdit, entityVM.FromDateEdit, entityVM.ToDateEdit, applicableYear, entityVM.LeaveApplicationID))
                return new CommonReturnViewModel { Success = false, Message = "You already have leave on selected dates" };

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
                .Include(x => x.ApprovalType)
                .Where(x =>
                    x.OrganizationID == offf.OrganizationID &&
                    (x.OrganizationBranchID == null || x.OrganizationBranchID == offf.OrganizationBranchID) &&
                    x.ApprovalType.ApprovalTypeName == "Leave Request Approval")
                .FirstOrDefaultAsync();

            if (approvalSettings == null)
                return new CommonReturnViewModel { Success = false, Message = "No active Leave Request Approval settings found." };

            int? approvalPersonId = null;

            var approvalFlow = new List<(int? id, bool isDesignation)>
    {
        (approvalSettings.FirstApprovalID, approvalSettings.IsDesignationOrEmpFirstApprovalID),
        (approvalSettings.SecondApprovalID, approvalSettings.IsDesignationOrEmpSecondApprovalID),
        (approvalSettings.ThirdApprovalID, approvalSettings.IsDesignationOrEmpThirdApprovalID)
    };

            foreach (var (id, isDesignation) in approvalFlow)
            {
                if (approvalPersonId != null) break;

                var resolvedId = await ResolveApprovalAsync(id, isDesignation, offf);
                if (!IsSelfApprovalBlocked(entityVM.EmployeeIDEdit, resolvedId, approvalSettings.AllowSelfApproval, approvalSettings.SelfExceptionApprovalID))
                {
                    approvalPersonId = resolvedId;
                }
            }

            if (approvalPersonId == null)
                return new CommonReturnViewModel { Success = false, Message = "No valid approver found." };

            await leaveRequest.BeginTransactionAsync();

            try
            {
                var (allowFallback, allowLWP) = await GetLeaveAdjustmentPolicyAsync();

                var lWP = await leaveTypes.AllActive().Where(x => x.LeaveTypeName == "LWP").Select(x => x.LeaveTypeID).FirstOrDefaultAsync();
                var annualLeaveType = await leaveTypes.AllActive().Where(x => x.LeaveTypeName == "Annual Leave").Select(x => x.LeaveTypeID).FirstOrDefaultAsync();

                var fromDate = entityVM.FromDateEdit ?? DateOnly.FromDateTime(DateTime.Today);
                var toDate = entityVM.ToDateEdit ?? DateOnly.FromDateTime(DateTime.Today);
                int totalRequestedDays = (toDate.DayNumber - fromDate.DayNumber) + 1;

                var leaveInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeIDEdit, entityVM.LeaveTypeIDEdit);
                decimal availableDays = leaveInfo?.LeaveDays ?? 0;
                int usedDays = (int)Math.Min(totalRequestedDays, availableDays);
                int remainingDays = totalRequestedDays - usedDays;

                var leaveTypeName = await leaveTypes.AllActive().Where(x => x.LeaveTypeID == entityVM.LeaveTypeIDEdit).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();
                var strictLeaveTypes = new List<string> { "Maternity Leave", "Paternity Leave" };

                if (remainingDays > 0 && strictLeaveTypes.Contains(leaveTypeName))
                    return new CommonReturnViewModel { Success = false, Message = $"{leaveTypeName} cannot be adjusted from other leave types." };

                // === 1. Update main leave record ===
                existing.IsFullDay = entityVM.IsFullDayEdit;
                existing.FromDate = fromDate;
                existing.ToDate = fromDate.AddDays(usedDays - 1);
                existing.PartialFromTime = entityVM.PartialFromTimeEdit;
                existing.PartialToTime = entityVM.PartialToTimeEdit;
                // existing.StatusID = entityVM.;
                existing.LeaveTypeID = entityVM.LeaveTypeIDEdit;
                // existing.IsGroupApplication = entityVM.IsGroupApplicationEdit;
                existing.Reason = entityVM.ReasonEdit;
                existing.ApprovalPersonID = approvalPersonId;
                existing.LIP = entityVM.LIP;
                existing.LMAC = entityVM.LMAC;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = entityVM.CreatedBy;

                await leaveRequest.UpdateAsync(existing);



                // === 2. Delete old fallback/LWP records if any ===
                var partialLeaves = await leaveRequest.All().Where(x => x.GroupApplicationID == entityVM.LeaveApplicationID).ToListAsync();
                foreach (var pl in partialLeaves)
                {
                    await leaveRequest.DeleteAsync(pl);
                }

                // === 3. Re-insert fallback/LWP records as needed ===
                var currentStart = fromDate.AddDays(usedDays);
                if (remainingDays > 0 && allowFallback)
                {
                    var fallbackTypes = await leaveTypes.AllActive()
                        .Where(x => x.IsActive && x.LeavePriorityId != null && x.LeaveTypeID != entityVM.LeaveTypeIDEdit && x.LeaveTypeID != annualLeaveType && x.LeaveTypeID != lWP)
                        .OrderBy(x => x.LeavePriorityId)
                        .Select(x => new { x.LeaveTypeID, x.LeaveTypeName })
                        .ToListAsync();

                    foreach (var fallback in fallbackTypes)
                    {
                        if (remainingDays <= 0) break;

                        var fallbackInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeIDEdit, fallback.LeaveTypeID);
                        int usedFallback = (int)Math.Min(remainingDays, fallbackInfo?.LeaveDays ?? 0);

                        if (usedFallback > 0)
                        {
                            var partial = new LeaveApplications
                            {
                                EmployeeID = entityVM.EmployeeIDEdit,
                                IsFullDay = entityVM.IsFullDayEdit,
                                FromDate = currentStart,
                                ToDate = currentStart.AddDays(usedFallback - 1),
                                PartialFromTime = entityVM.PartialFromTimeEdit,
                                PartialToTime = entityVM.PartialToTimeEdit,
                                // StatusID = entityVM.StatusID,
                                LeaveApplicableYear = DateTime.Now.Year,
                                CreatedAt = DateTime.Now,
                                CreatedBy = entityVM.CreatedBy,
                                LeaveTypeID = fallback.LeaveTypeID,
                                Reason = $"Exceeded original leave – adjusted using prioritized leave type ({fallback.LeaveTypeName})",
                                LIP = entityVM.LIP,
                                LMAC = entityVM.LMAC,
                                GroupApplicationID = existing.LeaveApplicationID,
                                ApprovalPersonID = approvalPersonId
                            };

                            await leaveRequest.AddAsync(partial);
                            await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, partial, partial.LeaveApplicationID, entityVM);

                            remainingDays -= usedFallback;
                            currentStart = currentStart.AddDays(usedFallback);
                        }
                    }
                }

                if (remainingDays > 0 && allowLWP)
                {
                    var lwpName = await leaveTypes.AllActive().Where(x => x.LeaveTypeID == lWP).Select(x => x.LeaveTypeName).FirstOrDefaultAsync();

                    var lwpEntity = new LeaveApplications
                    {
                        EmployeeID = entityVM.EmployeeIDEdit,
                        IsFullDay = entityVM.IsFullDayEdit,
                        FromDate = currentStart,
                        ToDate = currentStart.AddDays(remainingDays - 1),
                        PartialFromTime = entityVM.PartialFromTimeEdit,
                        PartialToTime = entityVM.PartialToTimeEdit,
                        // StatusID = entityVM.StatusID,
                        LeaveApplicableYear = DateTime.Now.Year,
                        CreatedAt = DateTime.Now,
                        CreatedBy = entityVM.CreatedBy,
                        LeaveTypeID = lWP,
                        Reason = $"Exceeded leave days – fallback to LWP ({lwpName})",
                        LIP = entityVM.LIP,
                        LMAC = entityVM.LMAC,
                        GroupApplicationID = existing.LeaveApplicationID,
                        ApprovalPersonID = approvalPersonId
                    };

                    await leaveRequest.AddAsync(lwpEntity);
                    await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, lwpEntity, lwpEntity.LeaveApplicationID, entityVM);
                }

                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel { Success = true, Message = "Updated Successfully." };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex);
                return new CommonReturnViewModel { Success = false, Message = "An error occurred while updating the leave request." };
            }
        }


        #endregion

        #region Delete Leave Request
        public async Task<CommonReturnViewModel> SoftDeleteLeaveRequest(DeleteRequestVM deleteRequestVM)
        {
            await leaveRequest.BeginTransactionAsync();
            try
            {
                var data = await leaveRequest.FindAsync(x => deleteRequestVM.Ids.Contains(x.LeaveApplicationID));
                if (data == null || data.Count == 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success=false,
                        Message = "No data found to delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<AddNewLeaveSave>>(
             JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                var targetIds = data.Select(x => (int?)x.LeaveApplicationID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = deleteRequestVM.LIP;
                    item.LMAC = deleteRequestVM.LMAC;
                    item.DeletedBy = deleteRequestVM.DeletedBy ?? null;
                }

                await leaveRequest.UpdateRangeAsync(data);
                await userInfoService.ActionLogDeleteAsync("Leave Settigs", ActionName.DataDeleted, null, beforeEntity, targetIds, deleteRequestVM);
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success=true,
                    Message = $"Deleted Successfully."
                    
                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion

        #region Get LeaveType Total Days
        public class LeaveBalanceResult
        {
            public decimal? LeaveDays { get; set; }
        }

        public async Task<LeaveBalanceResult?> GetLeaveTypeTotaldays2(int? employeeId, int? leaveTypeID)
        {
            var leaveBalance = await leaveBalances.AllActive()
                .Where(x => x.EmployeeID == employeeId && x.LeaveTypeID == leaveTypeID)
                .Select(x => new LeaveBalanceResult
                {
                    LeaveDays = x.TotalLeave - x.Taken
                })
                .FirstOrDefaultAsync();

            if (leaveBalance != null)
                return leaveBalance;

            var defaultLeave = await leaveTypes.AllActive()
                .Where(l => l.LeaveTypeID == leaveTypeID)
                .Select(l => new LeaveBalanceResult
                {
                    LeaveDays = l.LeaveDays
                }).FirstOrDefaultAsync();

            return defaultLeave;
        }

        public async Task<object> GetLeaveTypeTotaldays(int employeeId, int leaveTypeID)
        {

            //
            var usedUpLeaveTypeIds = await leaveBalances.AllActive()
        .Where(lb => lb.EmployeeID == employeeId && (lb.TotalLeave - lb.Taken) <= 0)
        .Select(lb => lb.LeaveTypeID).ToListAsync();

            var leaveTypess = await leaveTypes.AllActive()
                .Where(lt => !usedUpLeaveTypeIds.Contains(lt.LeaveTypeID)).ToListAsync();
            //
            var leaveBalance = await leaveBalances.AllActive()
                .Where(x => x.EmployeeID == employeeId && x.LeaveTypeID == leaveTypeID)
                .Select(x => new
                {
                    leaveDays = x.TotalLeave - x.Taken
                }).FirstOrDefaultAsync();

            if (leaveBalance != null)
            {
                return leaveBalance;
            }
            else
            {
                var defaultLeave = await leaveTypes.AllActive()
                    .Where(l => l.LeaveTypeID == leaveTypeID)
                    .Select(l => new
                    {
                        leaveDays = l.LeaveDays
                    }).FirstOrDefaultAsync();
                if (defaultLeave == null) return null;
                return defaultLeave;
            }
        }





        #endregion

        #region Get All Employee or Single

        public async Task<List<CommonSelectVM>> GetAllEmployee(string userId)
        {
            // Step 1: Get employeeId from the user
            var employeeId = await appDb.Users
                .Where(u => u.Id == userId)
                .Select(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            // Step 2: Get the role name
            var roleName = await (from user in appDb.Users
                                  join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                  join role in appDb.Roles on userRole.RoleId equals role.Id
                                  where user.Id == userId
                                  select role.Name)
                                 .FirstOrDefaultAsync();

            // Step 3: If not Admin, return only that employee
            if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                var data = await employee.AllActive()
                    .Where(x => x.EmployeeID == employeeId)
                    .Select(x => new CommonSelectVM
                    {
                        Id = x.EmployeeID,
                        Name = $"{x.FirstName} {x.LastName}"
                    }).ToListAsync();

                return data;
            }

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

        #region Get Leave oolicy IsCount or not 
        public async Task<List<GetLeavePolicyConfigurationVM>> GetLeavePolicyIsCountAsync()
        {
            await leavePolicyConfiguration.BeginTransactionAsync();

            try
            {
                var data = await leavePolicyConfiguration.AllActive()
                    .Select(x => new GetLeavePolicyConfigurationVM
                    {
                        IsWeekendCountedAsLeave = x.IsWeekendCountedAsLeave,
                        IsHolidayCountedAsLeave = x.IsHolidayCountedAsLeave,
                        IsAllowRequestForPastDates = x.IsAllowRequestForPastDates,

                        IsAllowRequestForFutureDays = x.IsAllowRequestForFutureDays,
                        AllowRequestForFutureDays = x.AllowRequestForFutureDays,

                        IsMaximumleaveDaysPerAplication = x.IsMaximumleaveDaysPerAplication,
                        MaximumleaveDaysPerAplication = x.MaximumleaveDaysPerAplication,

                        IsMaximumGapDaysBetweenAplications = x.IsMaximumGapDaysBetweenAplications,
                        MaximumGapDaysBetweenAplications = x.MaximumGapDaysBetweenAplications
                    }).ToListAsync();

                await leavePolicyConfiguration.CommitTransactionAsync();

                return data;
            }
            catch (Exception)
            {
                await leavePolicyConfiguration.RollbackTransactionAsync();
                throw;
            }
        }


        #endregion

        #region  SubsequentAsynce Leave Check

        public async Task<SubsequentWithRestrictionVM> SubsequentAsynceWithRestriction(int employeeId, DateTime fromDate, DateTime toDate)
        {


            if (employeeId == 0 || fromDate == null || toDate == null)
            {
                return new SubsequentWithRestrictionVM();
            }
            var normalizedFrom = fromDate.Date;
            var normalizedTo = toDate.Date;
            if (normalizedTo < normalizedFrom)
                throw new ArgumentException("toDate must be on or after fromDate");

            int totalDays = (int)(normalizedTo - normalizedFrom).TotalDays + 1;

            var isWeenedHoliday = await leavePolicyConfiguration.AllActive()
                .Select(x => new
                {
                    x.IsHolidayCountedAsLeave,
                    x.IsWeekendCountedAsLeave
                }).FirstOrDefaultAsync();

            if (isWeenedHoliday == null)
                return null;

            // Use HashSet to avoid duplicate dates
            HashSet<DateTime> uniqueDates = new HashSet<DateTime>();

            if (isWeenedHoliday.IsHolidayCountedAsLeave)
            {
                var holidaysInRange = await holidays.AllActive()
                    .Where(x => x.StartDate <= toDate && x.EndDate >= fromDate)
                    .ToListAsync();

                foreach (var h in holidaysInRange)
                {
                    var start = h.StartDate.GetValueOrDefault();
                    var end = h.EndDate.GetValueOrDefault();

                    var actualStart = start < fromDate ? fromDate : start;
                    var actualEnd = end > toDate ? toDate : end;

                    for (var date = actualStart; date <= actualEnd; date = date.AddDays(1))
                    {
                        uniqueDates.Add(date);
                    }
                }
            }

            if (isWeenedHoliday.IsWeekendCountedAsLeave)
            {
                var allWeekendDays = await weekedays.AllActive()
                    .Include(x => x.WeekendSetting)
                    .ToListAsync();


                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    int dayNumber = (int)date.DayOfWeek;

                    if (allWeekendDays.Any(w => w.WeekdayNumber == dayNumber))
                    {
                        uniqueDates.Add(date);
                    }
                }

            }

            var policy = await leavePolicyConfiguration.AllActive()
                                                            .Select(x => new
                                                            {
                                                                x.IsAllowRequestForPastDates,
                                                                x.IsAllowRequestForFutureDays,
                                                                x.AllowRequestForFutureDays,
                                                                x.IsMaximumleaveDaysPerAplication,
                                                                x.MaximumleaveDaysPerAplication,
                                                                x.IsMaximumGapDaysBetweenAplications,
                                                                x.MaximumGapDaysBetweenAplications
                                                            }).FirstOrDefaultAsync();
            if (policy == null)
                return null;

            var maxDaysMassage = "";
            if (policy.IsMaximumleaveDaysPerAplication == true && policy.MaximumleaveDaysPerAplication.HasValue)
            {
                if (totalDays > policy.MaximumleaveDaysPerAplication.Value)
                {
                    maxDaysMassage = $"Maximum {policy.MaximumleaveDaysPerAplication} days allowed.";
                }
            }

            string MaxGapdaysMessage = "";
            int appliableYear = DateTime.Now.Year;
            DateOnly fromDateOnly = DateOnly.FromDateTime(fromDate);
            var lastAcceptedLeaveToDate = await leaveRequest.AllActive()
                .Include(x => x.Status)
                .Where(x => x.EmployeeID == employeeId
                    && x.Status.StatusName == "APPROVED"
                    && x.LeaveApplicableYear == appliableYear
                    && x.ToDate <= fromDateOnly).OrderByDescending(x => x.ToDate) .Select(x => x.ToDate).FirstOrDefaultAsync();

            if (lastAcceptedLeaveToDate != default
                && policy.IsMaximumGapDaysBetweenAplications == true
                && policy.MaximumGapDaysBetweenAplications.HasValue)
            {

                
                int allowedGap = policy.MaximumGapDaysBetweenAplications.Value;
                // Calculate actual gap
                int gap = (fromDate.Date - lastAcceptedLeaveToDate.ToDateTime(TimeOnly.MinValue)).Days - 1;
                if (gap < allowedGap)
                {
                    // Calculate next valid from date
                    DateTime nextValidFrom = lastAcceptedLeaveToDate.ToDateTime(TimeOnly.MinValue).AddDays(allowedGap + 1);

                    MaxGapdaysMessage = $"Maximum {allowedGap} days gap is allowed between two applications. " +
                                        $"You can apply for leave starting from {nextValidFrom:dd/MM/yyyy}.";
                }
            }
            //
            // Get Leave Balance or Default Leave Days
            

            //
            return new SubsequentWithRestrictionVM
            {
                TotalDays = totalDays,
                TotalSubsequentDays = uniqueDates.Count,
                IsHolidayCountedAsLeave = isWeenedHoliday.IsHolidayCountedAsLeave,
                IsWeekendCountedAsLeave = isWeenedHoliday.IsWeekendCountedAsLeave,
                //
                IsAllowRequestForPastDates = policy.IsAllowRequestForPastDates,
                IsAllowRequestForFutureDays = policy.IsAllowRequestForFutureDays,
                AllowRequestForFutureDays = policy.AllowRequestForFutureDays,
                IsMaximumleaveDaysPerAplication = policy.IsMaximumleaveDaysPerAplication,
                MaximumleaveDaysPerAplication = policy.MaximumleaveDaysPerAplication,
                IsMaximumGapDaysBetweenAplications = policy.IsMaximumGapDaysBetweenAplications,
                MaximumGapDaysBetweenAplications = policy.MaximumGapDaysBetweenAplications,
                Message = maxDaysMassage,
                MaxGapdaysMessage= MaxGapdaysMessage,
               
            };
        }



        public async Task<SubsequentVM> SubsequentAsynce(DateTime fromDate, DateTime toDate)
        {

            var normalizedFrom = fromDate.Date;
            var normalizedTo = toDate.Date;
            if (normalizedTo < normalizedFrom)
                throw new ArgumentException("ToDate must be on or after fromDate");
           // var leaveapp=await leaveRequest.all
            int totalDays = (int)(normalizedTo - normalizedFrom).TotalDays + 1;

            var isWeenedHoliday = await leavePolicyConfiguration.AllActive()
                .Select(x => new
                {
                    x.IsHolidayCountedAsLeave,
                    x.IsWeekendCountedAsLeave
                }).FirstOrDefaultAsync();

            if (isWeenedHoliday == null)
                return null;

            // Use HashSet to avoid duplicate dates
            HashSet<DateTime> uniqueDates = new HashSet<DateTime>();

            if (isWeenedHoliday.IsHolidayCountedAsLeave)
            {
                var holidaysInRange = await holidays.AllActive()
                    .Where(x => x.StartDate <= toDate && x.EndDate >= fromDate)
                    .ToListAsync();

                foreach (var h in holidaysInRange)
                {
                    var start = h.StartDate.GetValueOrDefault();
                    var end = h.EndDate.GetValueOrDefault();

                    var actualStart = start < fromDate ? fromDate : start;
                    var actualEnd = end > toDate ? toDate : end;

                    for (var date = actualStart; date <= actualEnd; date = date.AddDays(1))
                    {
                        uniqueDates.Add(date);
                    }
                }
            }

            if (isWeenedHoliday.IsWeekendCountedAsLeave)
            {
                var allWeekendDays = await weekedays.AllActive()
                    .Include(x => x.WeekendSetting)
                    .ToListAsync();


                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    int dayNumber = (int)date.DayOfWeek; 

                    if (allWeekendDays.Any(w => w.WeekdayNumber == dayNumber))
                    {
                        uniqueDates.Add(date); 
                    }
                }

            }
            
            return new SubsequentVM
            {
                TotalDays = totalDays,
                TotalSubsequentDays = uniqueDates.Count,
                IsHolidayCountedAsLeave = isWeenedHoliday.IsHolidayCountedAsLeave,
                IsWeekendCountedAsLeave = isWeenedHoliday.IsWeekendCountedAsLeave,
            };
        }

        #endregion

        #region GetCompanies
        public  async Task<List<CommonSelectVM>> GetCompanies()
        {
            var data =await _organizationRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.OrganizationID,
                    Name = x.OrganizationName
                }).ToListAsync();
            
            return data;
        }
        #endregion

        #region GetDepartments
        public async Task<List<CommonSelectVM>> GetDepartments()
        {
            var data = await _departmentRepository.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.DepartmentID,
                    Name = x.DepartmentName
                }).ToListAsync();
            return data;
        }
        #endregion

        #region GetGroupedEmployees
        public async Task<List<MultiDropDown>> GetGroupedEmployees()
        {
            var data = await (from empOi in empoffi.AllActive().AsNoTracking()

                              join emp in employee.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join org in _organizationRepository.AllActive() on empOi.OrganizationID equals org.OrganizationID into orgGroup
                              from org in orgGroup.DefaultIfEmpty()

                              join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new MultiDropDown
                              {
                                  EmployeeID = empOi.EmployeeID ?? 0,
                                  EmployeeName = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  DepartmentName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion

        #region GetFilteredEmployees
        public async Task<List<MultiDropDown>> GetEmployeeByDepartment(List<int> departmentIds)
        {
            var query = from empOi in empoffi.AllActive().AsNoTracking()
                        join emp in employee.AllActive() on empOi.EmployeeID equals emp.EmployeeID into empGroup
                        from emp in empGroup.DefaultIfEmpty()
                        join dep in _departmentRepository.AllActive() on empOi.DepartmentID equals dep.DepartmentID into depGroup
                        from dep in depGroup.DefaultIfEmpty()
                        select new
                        {
                            empOi.EmployeeID,
                            emp.FirstName,
                            emp.LastName,
                            emp.EmployeeCode,
                            dep.DepartmentName,
                            empOi.OrganizationID,
                            empOi.DepartmentID
                        };

            if (departmentIds?.Any() == true)
                query = query.Where(x => departmentIds.Contains(x.DepartmentID ?? 0));

            return await query
                .Select(x => new MultiDropDown
                {
                    EmployeeID = x.EmployeeID ?? 0,
                    EmployeeName = $"{x.FirstName} {x.LastName} ({x.EmployeeCode})",
                    DepartmentName = x.DepartmentName
                }).ToListAsync();
        }
        #endregion

       

        #region GetDepartmentByCompany
        public async Task<List<MultiDropDown>> GetDepartmentByCompany(int id)
        {
            var data = await (from eoi in empoffi.AllActive()

                              where eoi.OrganizationID == id

                              join emp in employee.AllActive() on eoi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join org in _organizationRepository.AllActive() on eoi.OrganizationID equals org.OrganizationID into orgGrouop
                              from org in orgGrouop.DefaultIfEmpty()

                              join dep in _departmentRepository.AllActive() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new MultiDropDown
                              {
                                  DepartmentID = eoi.DepartmentID ?? 0,
                                  DepartmentName = dep.DepartmentName,
                              }).Distinct().ToListAsync();
            return data;
        }
        #endregion

        #region GetEmployeeByCompany
        public async Task<List<MultiDropDown>> GetEmployeeByCompany(int id)
        {
            var data = await (from eoi in empoffi.AllActive()

                              where eoi.OrganizationID == id

                              join emp in employee.AllActive() on eoi.EmployeeID equals emp.EmployeeID into empGroup
                              from emp in empGroup.DefaultIfEmpty()

                              join dep in _departmentRepository.AllActive() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                              from dep in depGroup.DefaultIfEmpty()

                              select new MultiDropDown
                              {
                                  EmployeeID = eoi.EmployeeID,
                                  EmployeeName = $"{emp.FirstName} {emp.LastName} ({emp.EmployeeCode})",
                                  DepartmentName = dep.DepartmentName
                              }).ToListAsync();
            return data;
        }
        #endregion

        #region Dispaly LeaveDays 
        public async Task<List<LeaveBalancesDisplayVM>> GetLeaveTypeBalancesForEmployee(int employeeId)
        {
            
            // Base query for leave balances
            var baseQuery = from lt in leaveTypes.AllActive()
                            join lb in leaveBalances.AllActive().Where(x => x.EmployeeID == employeeId)
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
                                    ? (lb.TotalLeave - lb.Taken)
                                    : (lt.LeaveDays ?? 0)
                            };



            return await baseQuery.ToListAsync();
        }
        #endregion

        #region Person Leave Step  

        public async Task<List<PersonLeaveStepVM>> GetByPersonLeaveStepVM(int leaveApplicationID)
        {
            try
            {
                var result = await (
                    from lb in leaveBaseApprovalHistory.AllActive()
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
                        ApprovedOrDeclineDate =DateTimeHelpers.FormatDateTime(lb.CreatedAt),

                    }).OrderBy(X=>X.ApproverStep).ToListAsync();

                return result ?? new List<PersonLeaveStepVM>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
