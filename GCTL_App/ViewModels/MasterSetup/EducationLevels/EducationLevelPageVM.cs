using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EducationLevels;

namespace GCTL_App.ViewModels.MasterSetup.EducationLevels
{
    public class EducationLevelPageVM : BaseViewModel
    {
        public EducationLevelVM Setup { get; set; } = new EducationLevelVM();
        public List<EducationLevelVM> List { get; set; } = new List<EducationLevelVM>();
    }
}
