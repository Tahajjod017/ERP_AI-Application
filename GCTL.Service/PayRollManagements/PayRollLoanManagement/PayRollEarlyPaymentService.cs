using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public class PayRollEarlyPaymentService:AppService<LoanDetails>, IPayRollEarlyPaymentService
    {
        private readonly IGenericRepository<LoanDetails> loanDetails;
        //private IGenericRepository<LoanInstallmentPeriods> loanInstallment;
        private readonly AppDbContext appDb;
        private readonly IGenericRepository<EmployeeOfficeInfo> empoffi;
        public readonly IGenericRepository<GCTL.Data.Models.Employees> employee;
        private readonly IGenericRepository<Departments> _departments;
        private readonly IGenericRepository<Organization> _organization;
        //private readonly IGenericRepository<LoanBaseApprovalHistory> loanBaseHistory;
        //private readonly IGenericRepository<ApprovalSettings> approvalSettingsRepository;
        //private readonly IGenericRepository<ApprovalTypes> approvalTypesRepository;
        //private readonly IGenericRepository<ApprovalDesignation> approvaldesignation;
        //private readonly IGenericRepository<Statuses> status;
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Loan> loan;
        public PayRollEarlyPaymentService(
            IGenericRepository<LoanDetails> loanDetails,
            //IGenericRepository<LoanInstallmentPeriods> loanInstallment,
            AppDbContext appDb,
            IGenericRepository<EmployeeOfficeInfo> empoffi,
            IGenericRepository<Data.Models.Employees> employee,
            //IGenericRepository<LoanBaseApprovalHistory> loanBaseHistory,
            //IGenericRepository<ApprovalSettings> approvalSettingsRepository,
            //IGenericRepository<ApprovalTypes> approvalTypesRepository,
            //IGenericRepository<ApprovalDesignation> approvaldesignation,
            //IGenericRepository<Statuses> status,
            IUserInfoService userInfoService,
            IGenericRepository<Loan> loan,
            IGenericRepository<Departments> departments,
            IGenericRepository<Organization> organization)
    : base(loanDetails)
        {
            this.loanDetails = loanDetails;
            //this.loanInstallment = loanInstallment;
            this.appDb = appDb;
            this.empoffi = empoffi;
            this.employee = employee;
            //this.loanBaseHistory = loanBaseHistory;
            //this.approvalSettingsRepository = approvalSettingsRepository;
            //this.approvalTypesRepository = approvalTypesRepository;
            //this.approvaldesignation = approvaldesignation;
            //this.status = status;
            _userInfoService = userInfoService;
            this.loan = loan;
            _departments = departments;
            _organization = organization;
        }

        #region Get LoanId
        public async Task<CommonReturnViewModel> GetPayRollEarlyPaymentAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid Employee ID"
                    };
                }

                var loanData = await loan.AllActive()
                    .Where(x => x.EmployeeID == id)
                    .Select(x => new SaveEarlyPaymentVM
                    {
                        LoanID = x.LoanID,
                        LoanAmount = x.LoanAmount,
                        LoanInstallmentPeriodID = x.LoanInstallmentPeriodID,
                        PaymentDateTime = DateTime.Now,
                        TenureMonth = x.LoanInstallmentPeriod != null ? x.LoanInstallmentPeriod.PeriodText ?? "" : "",
                        MonthlyEMI = (x.LoanAmount.HasValue && x.LoanInstallmentPeriod.PeriodValue.HasValue == true) ? decimal.Round(x.LoanAmount.Value / x.LoanInstallmentPeriod.PeriodValue.Value, 2) : 0,
                    }).FirstOrDefaultAsync();

                if (loanData == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan record not found for this employee."
                    };
                }

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = loanData
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        #endregion

        #region Save Early Payment
        public async Task<CommonReturnViewModel> SavePayRollEarlyPaymentAsync(SaveEarlyPaymentVM model)
        {
            if (model == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Model cannot be null.",
                    Errors = new List<string> { "Invalid input: Model is null." }
                };
            }

            if (model.LoanID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "LoanID not found.",
                    Errors = new List<string> { "Invalid LoanID: Must be greater than zero." }
                };
            }

            if (model.EarlyPayAmount <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid early payment amount.",
                    Errors = new List<string> { "EarlyPayAmount must be greater than zero." }
                };
            }

            if (model.PaymentDateTime == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid payment date.",
                    Errors = new List<string> { "PaymentDateTime cannot be null." }
                };
            }

            // Additional Validation: Check PaymentDateTime is not in the future
            if (model.PaymentDateTime ==null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid payment date.",
                    Errors = new List<string> { "PaymentDateTime cannot be in the future." }
                };
            }

            try
            {
                var loann = await loan.AllActive() // Assumes AllActive() filters DeletedAt == null
                .Include(l => l.LoanInstallmentPeriod)
                .FirstOrDefaultAsync(l => l.LoanID == model.LoanID);
                if (loan == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan not found.",
                        Errors = new List<string> { $"No loan found with LoanID {model.LoanID}." }
                    };
                }
                decimal currentInstallmentAmount = (loann.LoanAmount.HasValue && loann.LoanInstallmentPeriod?.PeriodValue.HasValue == true && loann.LoanInstallmentPeriod.PeriodValue.Value > 0)
                    ? decimal.Round(loann.LoanAmount.Value / loann.LoanInstallmentPeriod.PeriodValue.Value, 2)
                    : 0;

                if (currentInstallmentAmount <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid installment amount.",
                        Errors = new List<string> { "CurrentInstallmentAmount is zero or invalid due to missing LoanAmount, PeriodValue, or zero PeriodValue." }
                    };
                }
                if (model.EarlyPayAmount > currentInstallmentAmount)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Early payment exceeds current installment.",
                        Errors = new List<string> { $"EarlyPayAmount ({model.EarlyPayAmount}) exceeds CurrentInstallmentAmount ({currentInstallmentAmount})." }
                    };
                }

                decimal totalPaid = await loanDetails.AllActive()
                    .Where(ld => ld.LoanID == model.LoanID)
                    .SumAsync(ld => ld.EarlyPayAmount ?? 0);
                decimal remainingBalance = (loann.LoanAmount ?? 0) - totalPaid;

                if (model.EarlyPayAmount > remainingBalance)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Early payment exceeds remaining loan balance.",
                        Errors = new List<string> { $"EarlyPayAmount ({model.EarlyPayAmount}) exceeds remaining balance ({remainingBalance})." }
                    };
                }

                await loanDetails.BeginTransactionAsync();

                try
                {
                    var entity = new LoanDetails
                    {
                        LoanID = model.LoanID,
                        EarlyPayAmount = model.EarlyPayAmount,
                        CurrentInstallmentAmunt = currentInstallmentAmount,
                        PaymentDateTime = model.PaymentDateTime,
                        ReceivedByID = model.CreatedBy,
                        CreatedAt = DateTime.UtcNow, 
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };

                    await loanDetails.AddAsync(entity);
                    await loanDetails.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Early payment saved successfully.",
                        Data = entity
                    };
                }
                catch (Exception ex)
                {
                    await loanDetails.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Failed to save early payment.",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while initiating the transaction.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Update Data
        public async Task<CommonReturnViewModel> UpdatePayRollEarlyPaymentAsync(UpdateEarlyPayamentVM model)
        {
            if (model == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Model cannot be null.",
                    Errors = new List<string> { "Invalid input: Model is null." }
                };
            }

            if (model.LoanID <= 0 || model.LoanDetailsID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid Loan or LoanDetails ID.",
                    Errors = new List<string> { "LoanID and LoanDetailsID must be greater than zero." }
                };
            }

            if (model.EarlyPayAmount <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid early payment amount.",
                    Errors = new List<string> { "EarlyPayAmount must be greater than zero." }
                };
            }

            if (model.PaymentDateTime == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Payment date is required.",
                    Errors = new List<string> { "PaymentDateTime cannot be null." }
                };
            }

            try
            {
                await loanDetails.BeginTransactionAsync();

                // Fetch Loan and LoanDetails entities
                var loanEntity = await loan.AllActive()
                    .Include(l => l.LoanInstallmentPeriod)
                    .FirstOrDefaultAsync(l => l.LoanID == model.LoanID);

                if (loanEntity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan not found.",
                        Errors = new List<string> { $"No loan found with LoanID {model.LoanID}." }
                    };
                }

                var loanDetailEntity = await loanDetails.AllActive()
                    .FirstOrDefaultAsync(ld => ld.LoanDetailsID == model.LoanDetailsID);

                if (loanDetailEntity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Loan detail not found.",
                        Errors = new List<string> { $"No LoanDetails found with LoanDetailsID {model.LoanDetailsID}." }
                    };
                }

                // Calculate current installment
                decimal currentInstallmentAmount = (loanEntity.LoanAmount.HasValue && loanEntity.LoanInstallmentPeriod?.PeriodValue.HasValue == true && loanEntity.LoanInstallmentPeriod.PeriodValue.Value > 0)
                    ? decimal.Round(loanEntity.LoanAmount.Value / loanEntity.LoanInstallmentPeriod.PeriodValue.Value, 2)
                    : 0;

                if (model.EarlyPayAmount > currentInstallmentAmount)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Early payment exceeds current installment.",
                        Errors = new List<string> { $"EarlyPayAmount ({model.EarlyPayAmount}) exceeds CurrentInstallmentAmount ({currentInstallmentAmount})." }
                    };
                }

                // Check remaining balance
                decimal totalPaid = await loanDetails.AllActive()
                    .Where(ld => ld.LoanID == model.LoanID && ld.LoanDetailsID != model.LoanDetailsID)
                    .SumAsync(ld => ld.EarlyPayAmount ?? 0);
                decimal remainingBalance = (loanEntity.LoanAmount ?? 0) - totalPaid;

                if (model.EarlyPayAmount > remainingBalance)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Early payment exceeds remaining loan balance.",
                        Errors = new List<string> { $"EarlyPayAmount ({model.EarlyPayAmount}) exceeds remaining balance ({remainingBalance})." }
                    };
                }

                // Update fields
                loanDetailEntity.LoanID = model.LoanID;
                loanDetailEntity.EarlyPayAmount = model.EarlyPayAmount;
                loanDetailEntity.CurrentInstallmentAmunt = currentInstallmentAmount;
                loanDetailEntity.PaymentDateTime = model.PaymentDateTime;
                loanDetailEntity.UpdatedAt = DateTime.Now;
                loanDetailEntity.UpdatedBy = model.UpdatedBy;
                loanDetailEntity.LIP = model.LIP;
                loanDetailEntity.LMAC = model.LMAC;
                await loanDetails.UpdateAsync(loanDetailEntity);
                await loanDetails.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Early payment updated successfully.",
                    Data = loanDetailEntity
                };
            }
            catch (Exception ex)
            {
                await loanDetails.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Failed to update early payment.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Get All Table Datum
        public async Task<PaginationService<LoanDetails, EarlypaymentTableVM>.PaginationResult<EarlypaymentTableVM>> GetAllTableAsync(
int pageNumber = 1, int pageSize = 5, string searchTerm = "", string currentSortColumn = "", string currentSortOrder = "", string url = "", string userId = "", int? organizationId = null, List<int> departmentIds = null, List<int> employeeIds = null)
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

                // Base query from LoanDetails
                var query = loanDetails.AllActive()
                    .Include(ld => ld.Loan)
                        .ThenInclude(l => l.Employee)
                    .Include(ld => ld.Loan)
                        .ThenInclude(l => l.LoanInstallmentPeriod)
                    .OrderByDescending(ld => ld.LoanDetailsID)
                    .AsQueryable();

                // Filter for non-superadmin
                if (string.IsNullOrEmpty(roleName) || !string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(ld => ld.Loan.EmployeeID == employeeId);
                }

                // Filter by organization, department, employee
                var officeInfoQuery = empoffi.AllActive().AsQueryable();

                if (organizationId.HasValue)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => x.OrganizationID == organizationId)
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(ld => empIds.Contains(ld.Loan.EmployeeID ?? 0));
                }

                if (departmentIds?.Any() == true)
                {
                    var empIds = await officeInfoQuery
                        .Where(x => departmentIds.Contains(x.DepartmentID ?? 0))
                        .Select(x => x.EmployeeID)
                        .ToListAsync();

                    query = query.Where(ld => empIds.Contains(ld.Loan.EmployeeID ?? 0));
                }

                if (employeeIds?.Any() == true)
                {
                    query = query.Where(ld => employeeIds.Contains(ld.Loan.EmployeeID ?? 0));
                }

                // Sorting based on Loan (because LoanDetails doesn't have all these fields)
                Expression<Func<LoanDetails, object>> orderByExpression = currentSortColumn?.ToLower() switch
                {
                    "empid" => x => x.Loan.EmployeeID,
                    "empname" => x => x.Loan.Employee.FirstName + " " + x.Loan.Employee.LastName,
                    //"empdept" => x => x.Loan.Employee.emplooof.Department.DepartmentName, // Added department
                    "emploanamount" => x => x.Loan.LoanAmount ?? 0,
                    "empearlypayamount" => x => x.EarlyPayAmount ?? 0,
                    "paymentdate" => x => x.PaymentDateTime, // Added payment date
                    "receivedby" => x => x.CreatedByNavigation.FirstName + " " + x.CreatedByNavigation.LastName,
                    _ => x => x.LoanDetailsID
                };

                query = currentSortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(orderByExpression)
                    : query.OrderBy(orderByExpression);

                var result = await PaginationService<LoanDetails, EarlypaymentTableVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    currentSortColumn,
                    currentSortOrder,
                    term => ld =>
                        EF.Functions.Like(ld.Loan.LoanID.ToString(), $"%{term}%") ||
                         EF.Functions.Like(ld.Loan.Employee.FirstName + " " + ld.Loan.Employee.LastName, $"%{term}%") ||
                         //EF.Functions.Like(ld.Loan.Employee.EmployeeOfficialInfo.Department.DepartmentName ?? "", $"%{term}%") ||
                         EF.Functions.Like((ld.Loan.LoanAmount ?? 0).ToString(), $"%{term}%") ||
                         EF.Functions.Like((ld.EarlyPayAmount ?? 0).ToString(), $"%{term}%") ||
                         EF.Functions.Like(ld.CreatedByNavigation.FirstName + " " + ld.CreatedByNavigation.LastName, $"%{term}%"),
                    // (ld.PaymentDateTime.HasValue && EF.Functions.Like(
                    //ld.PaymentDateTime.Value.ToString("dd-MM-yyyy"), $"%{term}%")),
                    ld => new EarlypaymentTableVM
                    {
                        LoanDetailsID = ld.LoanDetailsID,
                        LoanID = ld.LoanID ?? 0,
                        EmployeeID = ld.Loan.EmployeeID,
                        CreatedByName = $"{ld.CreatedByNavigation.FirstName} {ld.CreatedByNavigation.LastName}",
                        LoanAmount = ld.Loan.LoanAmount ?? 0,
                        EarlyPayAmount = ld.EarlyPayAmount ?? 0,
                        PaymentDateTime = ld.PaymentDateTime?.ToString("dd-MM-yyyy"),
                        TenureMonth = ld.Loan.LoanInstallmentPeriod != null
                            ? ld.Loan.LoanInstallmentPeriod.PeriodText ?? ""
                            : "",
                        MonthlyEMI = (ld.Loan.LoanAmount.HasValue && ld.Loan.LoanInstallmentPeriod?.PeriodValue.HasValue == true)
                            ? decimal.Round(ld.Loan.LoanAmount.Value / ld.Loan.LoanInstallmentPeriod.PeriodValue.Value, 2)
                            : 0,
                        EmployeeName = ld.Loan.Employee != null
                            ? $"{ld.Loan.Employee.FirstName ?? ""} {ld.Loan.Employee.LastName ?? ""}".Trim()
                            : "",
                        EmployeeImage = (ld.Loan.Employee != null && !string.IsNullOrEmpty(ld.Loan.Employee.EmployeeImageFileName)) ? url + ld.Loan.Employee.EmployeeImageFileName : "",
                        EmployeeDepartment = empoffi.AllActive()
                            .Where(e => e.EmployeeID == ld.Loan.EmployeeID).Include(e => e.Department).Select(m => m.Department != null ? m.Department.DepartmentName ?? "" : "").FirstOrDefault() ?? ""
                    });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");

                return new PaginationService<LoanDetails, EarlypaymentTableVM>.PaginationResult<EarlypaymentTableVM>
                {
                    Data = new List<EarlypaymentTableVM>(),
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
                var employeesWithApprovedLoans = await (from e in employee.AllActive()
                                                        join l in loan.AllActive()
                                                        on e.EmployeeID equals l.EmployeeID
                                                        where l.IsFinalApproved == true
                                                        select new CommonSelectVM
                                                        {
                                                            Id = e.EmployeeID,
                                                            Name = $"{e.FirstName} {e.LastName}".Trim()
                                                        }).Distinct().ToListAsync();
                if (employeesWithApprovedLoans == null)
                {
                    throw new InvalidOperationException("Failed to retrieve employee data.");
                }
                return employeesWithApprovedLoans;
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

        #region Get Loan Details
        public async Task<CommonReturnViewModel> GetLaonDetailsAsync(int loanDetailsId)
        {
            try
            {
                if (loanDetailsId <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid LoanDetails ID"
                    };
                }

                // Get LoanDetails with its Loan
                var loanDetailData = await loanDetails.AllActive()
                    .Where(ld => ld.LoanDetailsID == loanDetailsId)
                    .Select(ld => new UpdateEarlyPayamentVM
                    {
                        LoanDetailsID = ld.LoanDetailsID,
                        LoanID = ld.LoanID ?? 0,
                        EmployeeIDs = ld.Loan.EmployeeID,
                        LoanAmount = ld.Loan.LoanAmount,
                        TenureMonth = ld.Loan.LoanInstallmentPeriod != null ? ld.Loan.LoanInstallmentPeriod.PeriodText ?? "" : "",
                        MonthlyEMI = (ld.Loan.LoanAmount.HasValue && ld.Loan.LoanInstallmentPeriod.PeriodValue.HasValue == true)
                            ? decimal.Round(ld.Loan.LoanAmount.Value / ld.Loan.LoanInstallmentPeriod.PeriodValue.Value, 2)
                            : 0,
                        PaymentDateTime = ld.PaymentDateTime,
                        EarlyPayAmount = ld.EarlyPayAmount
                    })
                    .FirstOrDefaultAsync();

                if (loanDetailData == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "LoanDetails record not found."
                    };
                }

                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = loanDetailData
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        #endregion


        #region GetDepartmentsByOrgId
        public async Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId)
        {
            var result = await (from dep in _departments.AllActive().AsNoTracking()

                                join org in _organization.AllActive().AsNoTracking() on dep.OrganizationID equals org.OrganizationID into orgGroup
                                from org in orgGroup.DefaultIfEmpty()

                                where dep.OrganizationID == orgId

                                select new CommonSelectVM
                                {
                                    Id = dep.DepartmentID,
                                    Name = dep.DepartmentName ?? "",
                                    GroupName = org.OrganizationName ?? ""
                                }).ToListAsync();

            return result;
        }
        #endregion
        public async Task<List<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? deptIds)
        {
            var baseQuery = empoffi.AllActive().AsNoTracking().Where(eoi => eoi.EmploymentStatusId == 1);

            if (orgId.HasValue && orgId.Value != 0)
                baseQuery = baseQuery.Where(eoi => eoi.OrganizationID == orgId.Value);

            if (branchIds != null && branchIds.Any())
                baseQuery = baseQuery.Where(eoi => eoi.OrganizationBranchID.HasValue && branchIds.Contains(eoi.OrganizationBranchID.Value));

            if (deptIds != null && deptIds.Any())
                baseQuery = baseQuery.Where(eoi => eoi.DepartmentID.HasValue &&
                                                   deptIds.Contains(eoi.DepartmentID.Value));

            var result = await (from eoi in baseQuery

                                join emp in employee.AllActive().AsNoTracking().Where(emp => emp.IsActive == true) on eoi.EmployeeID equals emp.EmployeeID

                                join dep in _departments.AllActive().AsNoTracking() on eoi.DepartmentID equals dep.DepartmentID into depGroup
                                from dep in depGroup.DefaultIfEmpty()
                                join l in loan.AllActive()
                                on emp.EmployeeID equals l.EmployeeID
                                where l.IsFinalApproved == true
                                select new CommonSelectVM
                                {
                                    Id = emp.EmployeeID,
                                    Name = $"{emp.FirstName ?? "-"} {emp.LastName ?? ""} ({emp.EmployeeCode ?? ""})",
                                    GroupName = dep.DepartmentName ?? ""
                                })
                               .ToListAsync();

            return result;
        }
    }
}
