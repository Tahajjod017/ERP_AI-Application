using GCTL.Core.ViewModels.MasterSetup.YearlyEndBonusType;

namespace GCTL_App.ViewModels.MasterSetup.YearlyEndBonusType
{
    public class YearlyEndBonusTypePageVM
    {
        public YearlyEndBonusTypeVM Setup { get; set; } = new YearlyEndBonusTypeVM();
        public List<YearlyEndBonusTypeVM> List { get; set; } = new List<YearlyEndBonusTypeVM>();
    }
}
