using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.EmployeeType;

namespace GCTL_App.ViewModels.MasterSetup.EmployeeType
{
    public class EmployeeTypesPageVM : BaseViewModel
    {
        public EmployeeTypesVM Setup { get; set; } = new EmployeeTypesVM();
        public List<EmployeeTypesVM> List { get; set; } = new List<EmployeeTypesVM>();
    }
}
