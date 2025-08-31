using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollLoanManagement
{
    public interface IPayRollLoanEntryService
    {
      Task<CommonReturnViewModel> SaveAsync(LoanSaveVM entityVM);
      Task<CommonReturnViewModel> UpdateAsync(LoanUpdateVM entityVM);
      Task<CommonReturnViewModel> DeleteAsync(DeleteRequestVM deleteRequestVM);
      Task<CommonReturnViewModel> GetByAsync(int id);
    }
}
