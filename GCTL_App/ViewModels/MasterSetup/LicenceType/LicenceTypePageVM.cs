using GCTL.Core.ViewModels.MasterSetup.LicenceType;

namespace GCTL_App.ViewModels.MasterSetup.LicenceType
{
    public class LicenceTypePageVM
    {
        public LicenceTypeVM Setup { get; set; } = new LicenceTypeVM();
        public List<LicenceTypeVM> List { get; set; } = new List<LicenceTypeVM>();
    }
}
