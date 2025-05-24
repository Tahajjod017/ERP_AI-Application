using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Grade;

namespace GCTL_App.ViewModels.MasterSetup.Grade
{
    public class GradePageVM : BaseViewModel
    {
        public GradeVM Setup { get; set; } = new GradeVM();
        public List<GradeVM> List { get; set; } = new List<GradeVM>();
    }
}
