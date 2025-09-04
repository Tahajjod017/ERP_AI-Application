using GCTL.Core.ViewModels.PayrollManagements.LoanManagement;

namespace GCTL_App.ViewModels.PayRollManagements.LoanManagent
{
    public class PayrollLoanEntryPageVM
    {
       public  LoanSaveVM Save {  get; set; }=new LoanSaveVM();

        public LoanUpdateVM Update { get; set; }=new LoanUpdateVM();
    }
}
