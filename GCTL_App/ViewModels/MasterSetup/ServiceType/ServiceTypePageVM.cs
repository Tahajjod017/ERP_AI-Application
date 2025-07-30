using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;

namespace GCTL_App.ViewModels.MasterSetup.ServiceType
{
    public class ServicTypePageVM : BaseViewModel
    {
        public ServiceTypeVM Setup { get; set; } = new ServiceTypeVM();
        public List<ServiceTypeVM> List { get; set; } = new List<ServiceTypeVM>();
    }
}