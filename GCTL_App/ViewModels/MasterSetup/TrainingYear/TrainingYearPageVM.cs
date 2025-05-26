using GCTL.Core.ViewModels.MasterSetup.TrainingYear;

namespace GCTL_App.ViewModels.MasterSetup.TrainingYear
{
    public class TrainingYearPageVM
    {
        public TrainingYearVM Setup { get; set; } = new TrainingYearVM();
        public List<TrainingYearVM> List { get; set; } = new List<TrainingYearVM>();
    }
}
