using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EmploymentNature;

namespace GCTL_App.ViewModels.MasterSetup.EmploymentNature
{
    public class EmploymentNaturePageVM : BaseViewModel
    {
        public EmploymentNatureVM Setup { get; set; } = new EmploymentNatureVM();
        public List<EmploymentNatureVM> List { get; set; } = new List<EmploymentNatureVM>();
    }
}
