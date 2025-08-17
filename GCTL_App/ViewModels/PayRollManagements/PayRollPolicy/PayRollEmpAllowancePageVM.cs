

using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;

namespace GCTL_App.ViewModels.PayRollManagements.PayRollPolicy
{
    public class PayRollEmpAllowancePageVM
    {
        public PayRollEmpAllowanceSaveVM Save { get; set; } = new PayRollEmpAllowanceSaveVM();

        public PayRollEmpAllowanceUpdate Update { get; set; }=new PayRollEmpAllowanceUpdate();
    }
}
