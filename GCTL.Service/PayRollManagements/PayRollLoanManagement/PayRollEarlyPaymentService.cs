using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public class PayRollEarlyPaymentService:AppService<LoanDetails>, IPayRollEarlyPaymentService
    {
        private readonly IGenericRepository<LoanDetails> loanDetails;
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
        private readonly IGenericRepository<Loan> loan;
        public PayRollEarlyPaymentService(
            IGenericRepository<LoanDetails> loanDetails,
            IGenericRepository<LoanInstallmentPeriods> loanInstallment,
            AppDbContext appDb,
            IGenericRepository<EmployeeOfficeInfo> empoffi,
            IGenericRepository<Data.Models.Employees> employee,
            IGenericRepository<LoanBaseApprovalHistory> loanBaseHistory,
            IGenericRepository<ApprovalSettings> approvalSettingsRepository,
            IGenericRepository<ApprovalTypes> approvalTypesRepository,
            IGenericRepository<ApprovalDesignation> approvaldesignation,
            IGenericRepository<Statuses> status,
            IUserInfoService userInfoService
,
            IGenericRepository<Loan> loan
            )
    : base(loanDetails)
        {
            this.loanDetails = loanDetails;
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
            this.loan = loan;
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
                // Retrieve Loan entity with LoanInstallmentPeriod
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

                // Calculate CurrentInstallmentAmount using the provided formula
                decimal currentInstallmentAmount = (loann.LoanAmount.HasValue && loann.LoanInstallmentPeriod?.PeriodValue.HasValue == true && loann.LoanInstallmentPeriod.PeriodValue.Value > 0)
                    ? decimal.Round(loann.LoanAmount.Value / loann.LoanInstallmentPeriod.PeriodValue.Value, 2)
                    : 0;

                // Validation: Check if CurrentInstallmentAmount is valid
                if (currentInstallmentAmount <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid installment amount.",
                        Errors = new List<string> { "CurrentInstallmentAmount is zero or invalid due to missing LoanAmount, PeriodValue, or zero PeriodValue." }
                    };
                }

                // Validation: Check if EarlyPayAmount exceeds CurrentInstallmentAmount
                if (model.EarlyPayAmount > currentInstallmentAmount)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Early payment exceeds current installment.",
                        Errors = new List<string> { $"EarlyPayAmount ({model.EarlyPayAmount}) exceeds CurrentInstallmentAmount ({currentInstallmentAmount})." }
                    };
                }

                // Validation: Check remaining loan balance
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


    }
}
