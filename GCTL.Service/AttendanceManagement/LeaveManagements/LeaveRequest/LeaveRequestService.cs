using Azure.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ActionLogVM;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveApprovalDecline;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignDefaultShift;
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
using System.Linq;
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
        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest, IGenericRepository<LeaveTypes> leaveTypes, IGenericRepository<Statuses> leaveStatuses, IUserInfoService userInfoService, IGenericRepository<Data.Models.Employees> employee, AppDbContext appDb, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<Holidays> holidays, IGenericRepository<WeekendSettings> weenkendsettings, IGenericRepository<WeekendDays> weekedays, IGenericRepository<LeaveBalances> leaveBalances, IGenericRepository<Organization> organizationRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<ApprovalSettings> approvalSettingsRepository, IGenericRepository<ApprovalTypes> approvalTypesRepository) : base(leaveRequest)
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
        }

        #region  Get Data All  Leave  Requyest
        public async Task<PaginationService<LeaveApplications, LeaveApplicationsList>.PaginationResult<LeaveApplicationsList>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
    List<int> departmentIds = null,
    List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await (from user in appDb.Users
                                      join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                      join role in appDb.Roles on userRole.RoleId equals role.Id
                                      where user.Id == userId
                                      select role.Name)
                                     .FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = leaveRequest.AllActive()
                    .Include(x => x.Employee)
                    .Include(x => x.Status)
                    .Include(x => x.LeaveType)
                    .OrderByDescending(x => x.LeaveApplicationID)
                    .AsQueryable();
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
                if (!string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
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
                var result = await PaginationService<LeaveApplications, LeaveApplicationsList>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%"),

                    b => new LeaveApplicationsList
                    {
                        //UserType = b.ActionLogID,
                        LeaveApplicationID = b.LeaveApplicationID,
                        StatusName = !string.IsNullOrEmpty(b.Status?.StatusName) ? b.Status.StatusName : "",
                        IsFullDay = b.IsFullDay,
                        LeaveType = b.LeaveType != null ? b.LeaveType.LeaveTypeName : "",
                        FromDate = DateOnly.FromDateTime(b.FromDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        ToDate = DateOnly.FromDateTime(b.ToDate.ToDateTime(TimeOnly.MinValue)).ToString("dd MMM yyyy"),
                        Period = b.IsFullDay ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1 : b.PartialFromTime.HasValue && b.PartialToTime.HasValue ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours : 0,
                        EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),


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

        //Duplicate applying date check
        private async Task<bool> HasOverlappingLeave(int? employeeId, DateOnly? from, DateOnly? to, int ? appliacbleYear)
        {
            var leaveStatusRejected = await leaveStatuses.AllActive()
                .Where(x => x.StatusName == "DECLINEED")
                .Select(x => x.StatusID)
                .ToListAsync();

            var retult = await leaveRequest.AllActive().AnyAsync(x =>
                x.EmployeeID == employeeId && 
                !leaveStatusRejected.Contains(x.Status.StatusID) && x.LeaveApplicableYear == appliacbleYear && 
                (
                    (from >= x.FromDate && from <= x.ToDate) ||
                    (to >= x.FromDate && to <= x.ToDate) ||
                    (from <= x.FromDate && to >= x.ToDate)
                )
            );
            return retult;
        }

        //Cross Leave utilization //While Applying Leave, Exceed Leave Balance check
        //private async Task<bool> ShouldDeductFromLWPAsync()
        //{
        //    var config = await leavePolicyConfiguration.AllActive()
        //        .Select(x => new
        //        {
        //            x.IsAllowCrossLeave,
        //            x.IsExceedLeaveBalance,
        //            x.EnableLeaveBalanceResetDate,
        //            ResetYear = x.LeaveBalanceResetDate.HasValue ? x.LeaveBalanceResetDate.Value.Year : (int?)null
        //        }).FirstOrDefaultAsync(x => x.IsAllowCrossLeave == false && x.IsExceedLeaveBalance == true);

        //    if (config != null)
        //    {
        //        Console.WriteLine("Policy Matched - EnableReset: " + config.EnableLeaveBalanceResetDate + " - Year: " + config.ResetYear);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Policy Check Result: Not matched");
        //    }

        //    return config != null;
        //}


        private async Task<(bool AllowFallback, bool AllowLWP)> GetLeaveAdjustmentPolicyAsync()
        {
            var config = await leavePolicyConfiguration.AllActive()
                .Select(x => new
                {
                    x.IsAllowCrossLeave,
                    x.IsExceedLeaveBalance,
                    x.EnableLeaveBalanceResetDate,
                    ResetYear = x.LeaveBalanceResetDate.HasValue ? x.LeaveBalanceResetDate.Value.Year : (int?)null
                })
                .FirstOrDefaultAsync();

            if (config != null)
            {
                Console.WriteLine("Policy Matched");
                // If both false => Only individual leave, no fallback
                if (!config.IsAllowCrossLeave && !config.IsExceedLeaveBalance)
                    return (false, false);

                // If exceed is allowed but no cross => only LWP allowed
                if (!config.IsAllowCrossLeave && config.IsExceedLeaveBalance)
                    return (false, true);

                // If both allowed => fallback and LWP allowed
                return (true, true);
            }

            // Default case: allow nothing
            return (false, false);
        }

        public async Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data cannot be null"
                };
            }

            int? applicableYear = DateTime.Now.Year;
            if (await HasOverlappingLeave(entityVM.EmployeeID, entityVM.FromDate, entityVM.ToDate, applicableYear))
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "You already have leave on selected dates"
                };
            }


            //
            var offf = await empoffi.AllActive().Where(x => x.EmployeeID == entityVM.EmployeeID).Select(x => new { x.EmployeeID, x.OrganizationID, x.OrganizationBranchID, x.DepartmentID, x.DesignationID, x.SeniorSupervisor, x.ImmediateSupervisor, x.HeadOfDepartmentId }).FirstOrDefaultAsync();

            if (offf == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee office info not found."
                };
            }

            var immediateSupervisorId = offf.ImmediateSupervisor;
            var seniorSupervisorId = offf.SeniorSupervisor;
            var headOfDepartmentId = offf.HeadOfDepartmentId;

            Console.WriteLine($"Immediate: {immediateSupervisorId}, Senior: {seniorSupervisorId}, HOD: {headOfDepartmentId}");

            var approvalTypes = await approvalTypesRepository.AllActive().Where(x => x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID).Select(x => new { x.ApprovalTypeID, x.ApprovalTypeName }).FirstOrDefaultAsync();
            if (approvalTypes == null)
            {
                return null;
            }
            var approvalSettings = await approvalSettingsRepository.AllActive().Include(x => x.ApprovalType).FirstOrDefaultAsync(x =>
                   x.OrganizationID == offf.OrganizationID || x.OrganizationBranchID == offf.OrganizationBranchID
                   && x.ApprovalType.ApprovalTypeName == "Leave Request Approval");

            bool isFirstApprover = approvalSettings?.FirstApprovalID == entityVM.UpdatedBy;
            bool isSecondApprover = approvalSettings?.SecondApprovalID == entityVM.UpdatedBy;
            bool isThirdApprover = approvalSettings?.ThirdApprovalID == entityVM.UpdatedBy;
            //
            //



            //
            if (!isFirstApprover && !isSecondApprover && !isThirdApprover)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "You are not authorized to approve this leave request."
                };
            }

            //



          //  Check permissions
            //bool isImmediateSupervisorPermitted =
            //    (approvalSettings.IsDesignationOrEmpFirstApprovalID && approvalSettings.IsDesignationOrEmpFirstApprovalID == immediateSupervisorId) ||
            //    (approvalSettings.IsEnableSecondApproval && approvalSettings.SecondApprovalID == immediateSupervisorId) ||
            //    (approvalSettings.IsEnableThirdApproval && approvalSettings.ThirdApprovalID == immediateSupervisorId);

            //bool isSeniorSupervisorPermitted =
            //    (approvalSettings.IsDesignationOrEmpFirstApprovalID && approvalSettings.FirstApprovalID == seniorSupervisorId) ||
            //    (approvalSettings.IsEnableSecondApproval && approvalSettings.SecondApprovalID == seniorSupervisorId) ||
            //    (approvalSettings.IsEnableThirdApproval && approvalSettings.ThirdApprovalID == seniorSupervisorId);

            //bool isHeadOfDeptPermitted =
            //    (approvalSettings.IsDesignationOrEmpFirstApprovalID && approvalSettings.FirstApprovalID == headOfDepartmentId) ||
            //    (approvalSettings.IsEnableSecondApproval && approvalSettings.SecondApprovalID == headOfDepartmentId) ||
            //    (approvalSettings.IsEnableThirdApproval && approvalSettings.ThirdApprovalID == headOfDepartmentId);

            // You can use the above booleans as needed
            //if (!isImmediateSupervisorPermitted && !isSeniorSupervisorPermitted && !isHeadOfDeptPermitted)
            //{
            //    return new CommonReturnViewModel
            //    {
            //        Success = false,
            //        Message = "None of the supervisors are authorized approvers."
            //    };
            //}

            await leaveRequest.BeginTransactionAsync();

            try
            {

                var (allowFallback, allowLWP) = await GetLeaveAdjustmentPolicyAsync();

                var lWP = await leaveTypes.AllActive()
                    .Where(x => x.LeaveTypeName == "LWP")
                    .Select(x => x.LeaveTypeID)
                    .FirstOrDefaultAsync();

                var annualLeaveType = await leaveTypes.AllActive()
                    .Where(x => x.LeaveTypeName == "Annual Leave")
                    .Select(x => x.LeaveTypeID)
                    .FirstOrDefaultAsync();

                var fromDate = entityVM.FromDate ?? DateOnly.FromDateTime(DateTime.Today);
                var toDate = entityVM.ToDate ?? DateOnly.FromDateTime(DateTime.Today);
                int totalRequestedDays = (toDate.DayNumber - fromDate.DayNumber) + 1;

                var leaveInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeID, entityVM.LeaveTypeID);
                decimal availableDays = leaveInfo?.LeaveDays ?? 0;

                int usedDaysFromCurrentType = (int)Math.Min(totalRequestedDays, availableDays);
                int remainingDays = totalRequestedDays - usedDaysFromCurrentType;

                // for strict leaveType

                // Get original leave name
                var originalLeaveTypeName = await leaveTypes.AllActive()
                    .Where(x => x.LeaveTypeID == entityVM.LeaveTypeID)
                    .Select(x => x.LeaveTypeName)
                    .FirstOrDefaultAsync();

                // Define strict leave types
                var strictLeaveTypes = new List<string> { "Maternity Leave", "Paternity Leave" };

                // If current leave type is strict, do not allow fallback
                if (remainingDays > 0 && strictLeaveTypes.Contains(originalLeaveTypeName))
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = $"{originalLeaveTypeName} cannot be adjusted from other leave types."
                    };
                }

                //
                int sequence = 0;

                // Save primary leave
                if (usedDaysFromCurrentType > 0)
                {
                    var entity = new LeaveApplications
                    {
                        EmployeeID = entityVM.EmployeeID,
                        IsFullDay = entityVM.IsFullDay,
                        FromDate = fromDate,
                        ToDate = fromDate.AddDays(usedDaysFromCurrentType - 1),
                        PartialFromTime = entityVM.PartialFromTime,
                        PartialToTime = entityVM.PartialToTime,
                        StatusID = entityVM.StatusID,
                        LeaveApplicableYear = DateTime.Now.Year,
                        CreatedAt = DateTime.Now,
                        CreatedBy = entityVM.CreatedBy,
                        LeaveTypeID = entityVM.LeaveTypeID,
                        IsGroupApplication = entityVM.IsGroupApplication,
                        Reason = entityVM.Reason,
                        LIP = entityVM.LIP,
                        LMAC = entityVM.LMAC
                    };

                    await leaveRequest.AddAsync(entity);
                    sequence = entity.LeaveApplicationID;

                    await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
                }
                //
                if (remainingDays > 0)
                {
                    var currentStartDate = fromDate.AddDays(usedDaysFromCurrentType);

                    // Step 1: Prioritized fallback leave types
                    if (allowFallback)
                    {
                        var prioritizedLeaves = await leaveTypes.AllActive()
                            .Where(x => x.IsActive &&
                                        x.LeavePriorityId != null &&
                                        x.LeaveTypeID != entityVM.LeaveTypeID &&
                                        x.LeaveTypeID != annualLeaveType &&
                                        x.LeaveTypeID != lWP)
                            .OrderBy(x => x.LeavePriorityId)
                            .Select(x => new { x.LeaveTypeID, x.LeaveTypeName })
                            .ToListAsync();

                        foreach (var leaveType in prioritizedLeaves)
                        {
                            if (remainingDays <= 0) break;

                            var fallbackInfo = await GetLeaveTypeTotaldays2(entityVM.EmployeeID, leaveType.LeaveTypeID);
                            decimal availableFallback = fallbackInfo?.LeaveDays ?? 0;
                            int usedFallback = (int)Math.Min(remainingDays, availableFallback);

                            if (usedFallback > 0)
                            {
                                var partialLeave = new LeaveApplications
                                {
                                    EmployeeID = entityVM.EmployeeID,
                                    IsFullDay = entityVM.IsFullDay,
                                    FromDate = currentStartDate,
                                    ToDate = currentStartDate.AddDays(usedFallback - 1),
                                    PartialFromTime = entityVM.PartialFromTime,
                                    PartialToTime = entityVM.PartialToTime,
                                    StatusID = entityVM.StatusID,
                                    LeaveApplicableYear = DateTime.Now.Year,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = entityVM.CreatedBy,
                                    LeaveTypeID = leaveType.LeaveTypeID,
                                    Reason = $"Exceeded original leave – adjusted using prioritized leave type ({leaveType.LeaveTypeName})",
                                    LIP = entityVM.LIP,
                                    LMAC = entityVM.LMAC,
                                    GroupApplicationID = sequence > 0 ? sequence : 0 // null
                                };

                                await leaveRequest.AddAsync(partialLeave);
                                await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, partialLeave, partialLeave.LeaveApplicationID, entityVM);

                                remainingDays -= usedFallback;
                                currentStartDate = currentStartDate.AddDays(usedFallback);
                            }
                        }
                    }

                    // Step 2: Fallback to LWP if allowed
                    if (remainingDays > 0 && allowLWP)
                    {
                        var lwpLeaveTypeName = await leaveTypes.AllActive()
                            .Where(x => x.LeaveTypeID == lWP)
                            .Select(x => x.LeaveTypeName)
                            .FirstOrDefaultAsync();

                        var lwpEntity = new LeaveApplications
                        {
                            EmployeeID = entityVM.EmployeeID,
                            IsFullDay = entityVM.IsFullDay,
                            FromDate = currentStartDate,
                            ToDate = currentStartDate.AddDays(remainingDays - 1),
                            PartialFromTime = entityVM.PartialFromTime,
                            PartialToTime = entityVM.PartialToTime,
                            StatusID = entityVM.StatusID,
                            LeaveApplicableYear = DateTime.Now.Year,
                            CreatedAt = DateTime.Now,
                            CreatedBy = entityVM.CreatedBy,
                            LeaveTypeID = lWP,
                            Reason = $"Exceeded leave days – fallback to LWP ({lwpLeaveTypeName})",
                            LIP = entityVM.LIP,
                            LMAC = entityVM.LMAC,
                            GroupApplicationID = sequence > 0 ? sequence : 0 
                        };

                        await leaveRequest.AddAsync(lwpEntity);
                        await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, lwpEntity, lwpEntity.LeaveApplicationID, entityVM);
                    }
                }

                //
                await leaveRequest.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Saved Successfully." 
                };
            }
            catch (Exception ex)
            {
                await leaveRequest.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the leave request."
                };
            }
        }


        
        //
        #endregion

        #region  Update Method
        public async Task<CommonReturnViewModel> UpdateLeaveRequestAsynce(LeaveApplicationEditVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data Can not be null"
                };
            }
           
            await leaveRequest.BeginTransactionAsync();

            try
            {
               

                var entity = await leaveRequest.GetByIdAsync(entityVM.LeaveApplicationID);
                if (entity == null)
                    return null;

                entity.EmployeeID = entityVM.EmployeeIDEdit;
                entity.LeaveTypeID = entityVM.LeaveTypeIDEdit;
               // entity.LeaveDays = entityVM.LeaveDaysEdit; // Add this property in your model if it exists

                entity.IsFullDay = entityVM.IsFullDayEdit;

                if (entityVM.IsFullDayEdit)
                {
                    entity.FromDate = entityVM.FromDateEdit ?? default;
                    entity.ToDate = entityVM.ToDateEdit ?? default;

                    // Clear partial day data
                    entity.PartialFromTime = null;
                    entity.PartialToTime = null;
                }
                else
                {
                    if (entityVM.ToDateFromDateCombinedEdit.HasValue)
                    {
                        var dateOnly = DateOnly.FromDateTime(entityVM.ToDateFromDateCombinedEdit.Value);
                        entity.FromDate = dateOnly;
                        entity.ToDate = dateOnly;
                    }

                    entity.PartialFromTime = entityVM.PartialFromTimeEdit;
                    entity.PartialToTime = entityVM.PartialToTimeEdit;
                }
                entity.Reason = entityVM.ReasonEdit ?? string.Empty;
                await leaveRequest.UpdateAsync(entity);
              //  await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
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
        public Task<List<CommonSelectVM>> GetCompanies()
        {
            var data = _organizationRepository.AllActive()
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


        #region SoftDeleteAsync
        public Task<MultiDropDown> SoftDeleteAsync(DeleteRequestVM model)
        {
            throw new NotImplementedException();
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
    }
}
