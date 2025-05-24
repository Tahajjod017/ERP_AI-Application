using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Degree;

namespace GCTL_App.ViewModels.MasterSetup.Degree
{
    public class DegreePageVM : BaseViewModel
    {
        public DegreeVM Setup { get; set; } = new DegreeVM();
        public List<DegreeVM> List { get; set; } = new List<DegreeVM>();
    }
}
