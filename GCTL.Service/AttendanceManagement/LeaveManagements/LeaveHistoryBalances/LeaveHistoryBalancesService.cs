using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveHistoryBalances;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Packaging.Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.LeaveManagements.LeaveHistoryBalances
{
    public class LeaveHistoryBalancesService : ILeaveHistoryBalancesService
    {
        private readonly IGenericRepository<LeaveApplications> leaveRequest;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        private readonly IGenericRepository<LeaveBalances> leaveBalances;
        private readonly IGenericRepository<LeaveBaseApprovalHistory> leaveBaseApprovalHistory;
        private readonly IGenericRepository<LeaveTypes> leaveTypesRepository;
        private readonly IGenericRepository<LeavePolicyConfiguration> leavePolicyConfigurationRepository;
        public LeaveHistoryBalancesService(IGenericRepository<LeaveApplications> leaveRequest, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi, IGenericRepository<LeaveBalances> leaveBalances, IGenericRepository<LeaveBaseApprovalHistory> leaveBaseApprovalHistory, IGenericRepository<LeaveTypes> leaveTypesRepository, IGenericRepository<LeavePolicyConfiguration> leavePolicyConfigurationRepository = null)
        {
            this.leaveRequest = leaveRequest;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.leaveBalances = leaveBalances;
            this.leaveBaseApprovalHistory = leaveBaseApprovalHistory;
            this.leaveTypesRepository = leaveTypesRepository;
            this.leavePolicyConfigurationRepository = leavePolicyConfigurationRepository;
        }

        #region Leave Balances Table 

        public async Task<PaginationService<LeaveBalances, LeaveBalancesGetVM>.PaginationResult<LeaveBalancesGetVM>> GetAllTableBalancesAsync(
  int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "",
  string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
  List<int> departmentIds = null, List<int> employeeIds = null, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            try
            {
                var employeeId = await appDb.Users
                    .Where(u => u.Id == userId)
                    .Select(e => e.EmployeeId)
                    .FirstOrDefaultAsync();

                var roleName = await (
                    from user in appDb.Users
                    join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                    join role in appDb.Roles on userRole.RoleId equals role.Id
                    where user.Id == userId
                    select role.Name).FirstOrDefaultAsync();

                // Get all leave balances
                var query = leaveBalances.AllActive()
                    .Include(x => x.Employee)
                    .Include(x => x.LeaveType)
                    .AsQueryable();

                // Filter: Non-SuperAdmin
                if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.EmployeeID == employeeId);
                }

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

                // Materialize the query for pivoting
                var leaveData = await query.ToListAsync();
                var leaveTypes = await leaveTypesRepository.AllActive().ToListAsync();
                var employees = leaveData.Select(x => x.Employee).Distinct().ToList();

                var leaveVMs = new List<LeaveBalancesGetVM>();

                foreach (var employee in employees)
                {
                    var vm = new LeaveBalancesGetVM
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        EmployeeImage = !string.IsNullOrEmpty(employee.EmployeeImageFileName) ? url + employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive()
                            .Where(e => e.EmployeeID == employee.EmployeeID)
                            .Include(e => e.Department)
                            .Select(e => e.Department.DepartmentName)
                            .FirstOrDefault()
                    };

                    foreach (var lt in leaveTypes)
                    {

                        //
                        var matchingLeave = leaveData.FirstOrDefault(l => l.EmployeeID == employee.EmployeeID && l.LeaveTypeID == lt.LeaveTypeID);

                        var companyPolicyHour = leavePolicyConfigurationRepository.AllActive().Select(x => x.WorkingHour).FirstOrDefault();

                        if (!companyPolicyHour.HasValue || companyPolicyHour == 0)
                        {
                            throw new InvalidOperationException("Company working hour policy is not configured or invalid. Please contact the administrator.");
                        }

                        decimal totalFullDaysFromPartial = 0;
                        decimal remainingPartialHours = 0;

                        if (matchingLeave?.TakenPartialHours > 0)
                        {
                            totalFullDaysFromPartial = Math.Floor(matchingLeave.TakenPartialHours.Value / companyPolicyHour.Value);
                            remainingPartialHours = matchingLeave.TakenPartialHours.Value % companyPolicyHour.Value; 
                        }

                        var totalLeave = matchingLeave?.TotalLeave ?? lt.LeaveDays ?? 0;
                        var fullDayTaken = (matchingLeave?.Taken ?? 0) + totalFullDaysFromPartial;
                        var partialTakenInDays = remainingPartialHours / (companyPolicyHour.HasValue ? (decimal)companyPolicyHour.Value : 1);

                        var taken = fullDayTaken + partialTakenInDays;
                        var remaining = totalLeave - taken;

                        //
                        switch (lt.LeaveTypeName)
                        {
                            case "Annual Leave":
                                vm.AnnualTaken = taken;
                                vm.AnnualRemaining = remaining;
                                break;
                            case "Casual Leave":
                                vm.CasualTaken = taken;
                                vm.CasualRemaining = remaining;
                                break;
                            case "Sick Leave":
                                vm.MedicalTaken = taken;
                                vm.MedicalRemaining = remaining;
                                break;
                            case "Maternity Leave":
                                vm.MaternityTaken = taken;
                                vm.MaternityRemaining = remaining;
                                break;
                            case "Paternity Leave":
                                vm.PaternityTaken = taken;
                                vm.PaternityRemaining = remaining;
                                break;
                        }
                    }

                    leaveVMs.Add(vm);
                }

                // Optional search
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    leaveVMs = leaveVMs
                        .Where(x => x.EmployeeName.ToLower().Contains(term) || (x.EmployeeDepartment?.ToLower().Contains(term) ?? false))
                        .ToList();
                }

                // Optional sort
                if (!string.IsNullOrEmpty(currentSortColumn))
                {
                    leaveVMs = currentSortColumn switch
                    {
                        "EmployeeName" => (currentSortOrder == "asc" ? leaveVMs.OrderBy(x => x.EmployeeName) : leaveVMs.OrderByDescending(x => x.EmployeeName)).ToList(),
                        "EmployeeDepartment" => (currentSortOrder == "asc" ? leaveVMs.OrderBy(x => x.EmployeeDepartment) : leaveVMs.OrderByDescending(x => x.EmployeeDepartment)).ToList(),
                        _ => leaveVMs
                    };
                }

                // Pagination
                var totalItems = leaveVMs.Count;
                var pagedData = leaveVMs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                return new PaginationService<LeaveBalances, LeaveBalancesGetVM>.PaginationResult<LeaveBalancesGetVM>
                {
                    Data = pagedData,
                    TotalCount = totalItems,
                    PaginationInfo = new PaginationService<LeaveBalances, LeaveBalancesGetVM>.PaginationInfo
                    {
                        CurrentPage = pageNumber,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        PageNumbers = Enumerable.Range(1, totalPages).ToList(),
                        StartItem = totalItems == 0 ? 0 : (pageNumber - 1) * pageSize + 1,
                        EndItem = Math.Min(pageNumber * pageSize, totalItems)
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new PaginationService<LeaveBalances, LeaveBalancesGetVM>.PaginationResult<LeaveBalancesGetVM>
                {
                    Data = new List<LeaveBalancesGetVM>(),
                    TotalCount = 0
                };
            }

        }

        #endregion


        #region Leave History Table List
        public async Task<PaginationService<LeaveApplications, LeaveHistoryGetVM>.PaginationResult<LeaveHistoryGetVM>> GetAllTableHistoryAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? leaveTypeID = null, int? statusID = null, int? organizationId = null,
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
                    .OrderByDescending(x => x.LeaveApplicationID).AsQueryable();
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
                var result = await PaginationService<LeaveApplications, LeaveHistoryGetVM>.GetPaginatedData(


                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,

                    currentSortColumn,
                    currentSortOrder,

                    term => b => EF.Functions.Like(b.LeaveApplicationID.ToString(), $"%{term}%"),

                    b => new LeaveHistoryGetVM
                    {
                        
                        LeaveApplicationID = b.LeaveApplicationID,
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

                return new PaginationService<LeaveApplications, LeaveHistoryGetVM>.PaginationResult<LeaveHistoryGetVM>
                {
                    Data = new List<LeaveHistoryGetVM>(),
                    TotalCount = 0
                };
            }
        }
        #endregion

    }
}
