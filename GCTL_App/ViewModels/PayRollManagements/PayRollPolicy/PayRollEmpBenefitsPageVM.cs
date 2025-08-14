using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeBenefitsVM;

namespace GCTL_App.ViewModels.PayRollManagements.PayRollPolicy
{
    public class PayRollEmpBenefitsPageVM
    {
        public PayRollEmpBenefitsSaveVM Save { get; set; } = new PayRollEmpBenefitsSaveVM();
        public PayRollEmpBenefitsUpdate Update { get; set; }=new PayRollEmpBenefitsUpdate();
    }
}
