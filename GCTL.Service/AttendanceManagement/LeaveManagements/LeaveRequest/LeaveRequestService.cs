using Azure.Core;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ActionLogVM;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
        //
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
       
     
        //
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<Holidays> holidays;
        private readonly IGenericRepository<WeekendSettings> weenkendsettings;
        private readonly IGenericRepository<WeekendDays> weekedays;
        private readonly IGenericRepository<LeaveBalances> leaveBalances;
        public LeaveRequestService(IGenericRepository<LeaveApplications> leaveRequest, IGenericRepository<LeaveTypes> leaveTypes, IGenericRepository<Statuses> leaveStatuses, IUserInfoService userInfoService, IGenericRepository<Data.Models.Employees> employee, AppDbContext appDb, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfiguration, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<Holidays> holidays, IGenericRepository<WeekendSettings> weenkendsettings, IGenericRepository<WeekendDays> weekedays, IGenericRepository<LeaveBalances> leaveBalances, IGenericRepository<Organization> organizationRepository , IGenericRepository<Departments> departmentRepository ) : base(leaveRequest)
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
                        Period = b.IsFullDay ? (b.ToDate.DayNumber - b.FromDate.DayNumber) + 1 : b.PartialFromTime.HasValue && b.PartialToTime.HasValue ? (int)(b.PartialToTime.Value - b.PartialFromTime.Value).TotalHours : 0,
                        EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault()

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
                SubsequentVM subsequent = null;

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

        //
        private async Task<bool> HasOverlappingLeave(int? employeeId, DateOnly? from, DateOnly? to)
        {
            var leaveStatusRejected = await leaveStatuses.AllActive()
                .Where(x => x.StatusName == "DECLINEED")
                .Select(x => x.StatusID)
                .ToListAsync();

            var retult = await leaveRequest.AllActive().AnyAsync(x =>
                x.EmployeeID == employeeId &&
                !leaveStatusRejected.Contains(x.Status.StatusID) &&
                (
                    (from >= x.FromDate && from <= x.ToDate) ||
                    (to >= x.FromDate && to <= x.ToDate) ||
                    (from <= x.FromDate && to >= x.ToDate)
                )
            );
            return retult;
        }


        //
        public async Task<CommonReturnViewModel> SaveLeaveRequestAsync(LeaveApplicationsRequestVM entityVM)
        {
            if (entityVM == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Data Can not be null"
                };
            }
            // Duplicate Date Check
            if (await HasOverlappingLeave(entityVM.EmployeeID, entityVM.FromDate, entityVM.ToDate))
            { 
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "You already have leave on selected dates"
                };

            }
            //
       
        await leaveRequest.BeginTransactionAsync();
            
            try
            {


                var entity = new LeaveApplications
                {
                    EmployeeID = entityVM.EmployeeID,
                    IsFullDay = entityVM.IsFullDay,
                    FromDate = entityVM.FromDate ?? DateOnly.FromDateTime(DateTime.Today),
                    ToDate = entityVM.ToDate ?? DateOnly.FromDateTime(DateTime.Today),
                    PartialFromTime = entityVM.PartialFromTime,
                    PartialToTime = entityVM.PartialToTime,
                    StatusID = entityVM.StatusID,
                    LeaveApplicableYear = DateTime.Now.Year,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LeaveTypeID = entityVM.LeaveTypeID,
                    Reason = entityVM.Reason,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };

                await leaveRequest.AddAsync(entity);
                await userInfoService.ActionLogAsync("Leave Apply", ActionName.DataAdd, null, entity, entity.LeaveApplicationID, entityVM);
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
        public async Task<object> GetLeaveTypeTotaldays(int employeeId, int leaveTypeID)
        {

            //
            var usedUpLeaveTypeIds = await leaveBalances.AllActive()
        .Where(lb => lb.EmployeeID == employeeId && (lb.TotalLeave - lb.Taken) <= 0)
        .Select(lb => lb.LeaveTypeID)
        .ToListAsync();

            var leaveTypess = await leaveTypes.AllActive()
                .Where(lt => !usedUpLeaveTypeIds.Contains(lt.LeaveTypeID))
                .ToListAsync();
            //
            var leaveBalance = await leaveBalances.AllActive()
                .Where(x => x.EmployeeID == employeeId && x.LeaveTypeID == leaveTypeID)
                .Select(x => new
                {
                    leaveDays = x.TotalLeave - x.Taken
                })
                .FirstOrDefaultAsync();

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
                var data = await leavePolicyConfiguration.AllActive().Select(x => new GetLeavePolicyConfigurationVM
                {
                    IsWeekendCountedAsLeave = x.IsWeekendCountedAsLeave,
                    IsHolidayCountedAsLeave = x.IsHolidayCountedAsLeave,
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

        public async Task<SubsequentVM> SubsequentAsynce(DateTime fromDate, DateTime toDate)
        {

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
                // fromDate and toDate are DateTime variables passed to the method
                var allWeekendDays = await weekedays.AllActive()
                    .Include(x => x.WeekendSetting)
                    .ToListAsync();

                //var uniqueDates = new HashSet<DateTime>();

                // Loop through date range
                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    int dayNumber = (int)date.DayOfWeek; // Sunday = 0, Monday = 1, ..., Saturday = 6

                    // Check if this dayNumber exists in the WeekendDays table
                    if (allWeekendDays.Any(w => w.WeekdayNumber == dayNumber))
                    {
                        uniqueDates.Add(date); // Add the date to the result if it's a weekend
                    }
                }

            }

            return new SubsequentVM
            {
                TotalDays = totalDays,
                TotalSubsequentDays = uniqueDates.Count,
                IsHolidayCountedAsLeave = isWeenedHoliday.IsHolidayCountedAsLeave,
                IsWeekendCountedAsLeave = isWeenedHoliday.IsWeekendCountedAsLeave
            };
        }




        #endregion


        //

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


        //

        



        //
    }
}
