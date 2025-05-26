using GCTL.Core.ViewModels.MasterSetup.ServiceYear;

namespace GCTL_App.ViewModels.MasterSetup.ServiceYear
{
    public class ServiceYearPageVM
    {
        public ServiceYearVM Setup { get; set; } = new ServiceYearVM();
        public List<ServiceYearVM> List { get; set; } = new List<ServiceYearVM>();
    }
}
