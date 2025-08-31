using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public class PayRollLoanEntryService:AppService<Loan>, IPayRollLoanEntryService
    {
        private readonly IGenericRepository<Loan> loanRepository;

        public PayRollLoanEntryService(IGenericRepository<Loan> loanRepository):base(loanRepository) 
        {
            this.loanRepository = loanRepository;
        }
        public async Task<CommonReturnViewModel> SaveAsync(LoanSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();
            try
            {
                await loanRepository.BeginTransactionAsync();
                Loan loan = new Loan
                {
                    EmployeeID = 22,
                    LoanInstallmentPeriodID = entityVM.LoanInstallmentPeriodID,
                    LoanAmount = entityVM.LoanAmount,
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
        public Task<CommonReturnViewModel> GetByAsync(int id)
        {
            throw new NotImplementedException();
        }
        public Task<CommonReturnViewModel> DeleteAsync(DeleteRequestVM deleteRequestVM)
        {
            throw new NotImplementedException();
        }
       
    }
}
