using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;

namespace GCTL_App.ViewModels.PayRollManagements.PayRollSettings
{
    public class PayRolltaxpercentagePageVM
    {
        public PayRollTaxPercentageSaveVM SaveVM { get; set; }= new PayRollTaxPercentageSaveVM();
        public PayRollTaxpercentageUpdateVM UpdateVM { get; set; } = new PayRollTaxpercentageUpdateVM();
    }
}
