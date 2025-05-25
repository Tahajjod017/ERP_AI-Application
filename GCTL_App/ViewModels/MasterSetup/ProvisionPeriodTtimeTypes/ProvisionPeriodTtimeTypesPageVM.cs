using GCTL.Core.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes;

namespace GCTL_App.ViewModels.MasterSetup.ProvisionPeriodTtimeTypes
{
    public class ProvisionPeriodTtimeTypesPageVM
    {
        public ProvisionPeriodTtimeTypesVM Setup { get; set; } = new ProvisionPeriodTtimeTypesVM();
        public List<ProvisionPeriodTtimeTypesVM> List { get; set; } = new List<ProvisionPeriodTtimeTypesVM>();
    }
}
