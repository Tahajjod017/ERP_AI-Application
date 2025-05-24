using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EducationBoard;

namespace GCTL_App.ViewModels.MasterSetup.EducationBoard
{
    public class EducationBoardPageVM : BaseViewModel
    {
        public EducationBoardVM Setup { get; set; } = new EducationBoardVM();
        public List<EducationBoardVM> List { get; set; } = new List<EducationBoardVM>();
    }
}
