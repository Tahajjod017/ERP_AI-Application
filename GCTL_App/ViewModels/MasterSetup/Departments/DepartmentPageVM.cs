using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Departments;

namespace GCTL_App.ViewModels.MasterSetup.Departments
{
    public class DepartmentPageVM : BaseViewModel
    {
        public DepartmentVM Setup { get; set; } = new DepartmentVM();
        public List<DepartmentVM> List { get; set; } = new List<DepartmentVM>();
    }
}
