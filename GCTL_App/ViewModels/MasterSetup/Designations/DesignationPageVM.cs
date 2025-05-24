using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Designations;

namespace GCTL_App.ViewModels.MasterSetup.Designations
{
    public class DesignationPageVM : BaseViewModel
    {
        public DesignationVM Setup { get; set; } = new DesignationVM();
        public List<DesignationVM> List { get; set; } = new List<DesignationVM>();
    }
}
