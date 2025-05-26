using GCTL.Core.ViewModels.MasterSetup.PassingYear;

namespace GCTL_App.ViewModels.MasterSetup.PassingYear
{
    public class PassingYearPageVM
    {
        public PassingYearVM Setup { get; set; } = new PassingYearVM();
        public List<PassingYearVM> List { get; set; } = new List<PassingYearVM>();
    }
}
