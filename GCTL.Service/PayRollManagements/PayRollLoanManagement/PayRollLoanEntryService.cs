using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.MasterSetup.Statuses;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement.LeaveManagements;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public class PayRollLoanEntryService:AppService<Loan>, IPayRollLoanEntryService
    {
        private readonly IGenericRepository<Loan> loanRepository;
        private IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        public PayRollLoanEntryService(IGenericRepository<Loan> loanRepository, IGenericRepository<LoanInstallmentPeriods> loanInstallment, AppDbContext appDb, IGenericRepository<EmployeeOfficeInfo> empoffi) : base(loanRepository)
        {
            this.loanRepository = loanRepository;
            this.loanInstallment = loanInstallment;
            this.appDb = appDb;
            this.empoffi = empoffi;
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
                await loanRepository.BeginTransactionAsync();
                Loan loan = new Loan
                {
                    EmployeeID = entityVM.EmployeeIDs.FirstOrDefault(),
                    LoanInstallmentPeriodID = loanInstallmentPeriodID,
                    LoanAmount = entityVM.LoanAmount.Value,
                    IssueDate = entityVM.IssueDate,
                    StartDate = entityVM.StartDate,
                    CreatedAt = DateTime.Now,
                    CreatedBy = entityVM.CreatedBy,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC
                };

                await loanRepository.AddAsync(loan);
                await loanRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Loan Saved Successfully.";
                result.Data = loan; // Optionally return the saved loan
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
        #endregion

        #region  Delete 
        public Task<CommonReturnViewModel> DeleteAsync(DeleteRequestVM deleteRequestVM)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginationService<Loan, LoanViewGetAllVM>.PaginationResult<LoanViewGetAllVM>> GetAllTableAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null)
        {
            try
            {

                var employeeId = await appDb.Users.Where(u => u.Id == userId).Select(e => e.EmployeeId).FirstOrDefaultAsync();
                var roleName = await(from user in appDb.Users
                                     join userRole in appDb.UserRoles on user.Id equals userRole.UserId
                                     join role in appDb.Roles on userRole.RoleId equals role.Id
                                     where user.Id == userId
                                     select role.Name).FirstOrDefaultAsync();

                // 🔹 Step 3: Base query with includes
                var query = loanRepository.AllActive().Include(x => x.Employee).Include(x=>x.LoanInstallmentPeriod).OrderByDescending(x => x.LoanID).AsQueryable();
                if (query == null)
                {
                    throw new InvalidOperationException("query source is null.");
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
                Expression<Func<Loan, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "employeename" => x => x.Employee.FirstName + " " + x.Employee.LastName,
                   
                    _ => x => x.LoanID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);


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
                      EF.Functions.Like(b.Employee.FirstName + " " + b.Employee.LastName, $"%{term}%"),



                    //b => new LoanViewGetAllVM
                    //{
                    //    LoanID = b.LoanID,
                    //    EmployeeID = b.EmployeeID,
                    //    LoanAmount = b.LoanAmount.Value,
                    //    TenureMonth =b.LoanInstallmentPeriod.PeriodText,
                    //    MonthlyEMI = b.LoanInstallmentPeriodID.HasValue && b.LoanAmount.HasValue ? b.LoanAmount.Value / b.LoanInstallmentPeriod.PeriodValue.Value : 0,
                    //    EmployeeName = $"{b.Employee.FirstName} {b.Employee.LastName}",
                    //    EmployeeImage = !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName) ? url + b.Employee.EmployeeImageFileName : "",
                    //    EmployeeDepartment = empoffi.AllActive().Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department.DepartmentName).FirstOrDefault(),

                    //});
                    b => new LoanViewGetAllVM
                    {
                        LoanID = b.LoanID,
                        EmployeeID = b.EmployeeID,
                        LoanAmount = b.LoanAmount ?? 0, // safely default to 0 if null
                        TenureMonth = b.LoanInstallmentPeriod != null ? b.LoanInstallmentPeriod.PeriodText : "",
                        MonthlyEMI = (b.LoanAmount.HasValue && b.LoanInstallmentPeriod?.PeriodValue.HasValue == true)
                 ? b.LoanAmount.Value / b.LoanInstallmentPeriod.PeriodValue.Value : 0,
                   EmployeeName = b.Employee != null ? $"{b.Employee.FirstName} {b.Employee.LastName}" : "",

                    EmployeeImage = (b.Employee != null && !string.IsNullOrEmpty(b.Employee.EmployeeImageFileName))? url + b.Employee.EmployeeImageFileName: "",

                        EmployeeDepartment = empoffi.AllActive()
                        .Where(e => e.EmployeeID == b.EmployeeID).Include(e => e.Department).Select(m => m.Department != null ? m.Department.DepartmentName : "").FirstOrDefault()
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


    }
}
