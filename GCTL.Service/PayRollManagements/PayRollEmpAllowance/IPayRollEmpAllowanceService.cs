using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpAllowance
{
    public interface IPayRollEmpAllowanceService
    {
        Task<CommonReturnViewModel>SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM);
    }
}
