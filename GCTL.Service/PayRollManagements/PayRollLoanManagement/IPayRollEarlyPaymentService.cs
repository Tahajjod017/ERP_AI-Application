using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayRollEarlyLoanpayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public interface IPayRollEarlyPaymentService
    {
        Task<CommonReturnViewModel> GetPayRollEarlyPaymentAsync(int id);
        Task<CommonReturnViewModel> SavePayRollEarlyPaymentAsync(SaveEarlyPaymentVM model);
    }
}
