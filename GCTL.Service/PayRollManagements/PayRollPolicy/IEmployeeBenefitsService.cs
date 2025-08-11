using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public interface IEmployeeBenefitsService
    {
        Task<CommonReturnViewModel> SaveEmployeeBenefits(PayRollEmpBenefitsSaveVM entityVM);
    }
}
