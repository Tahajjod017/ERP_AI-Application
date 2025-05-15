using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MenuTab;

namespace GCTL_NBR.ViewModels.MenuTab
{
    public class MenuTabPageVM : BaseViewModel
    {
        public MenuTabVM Setup { get; set; } = new MenuTabVM();
        public List<MenuTabVM> List { get; set; } = new List<MenuTabVM>();
    }
}
