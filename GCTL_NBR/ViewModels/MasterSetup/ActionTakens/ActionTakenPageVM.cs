using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;

namespace GCTL_NBR.ViewModels.MasterSetup.ActionTakens
{
    public class ActionTakenPageVM : BaseViewModel
    {
        public ActionTakenVM Setup { get; set; } = new ActionTakenVM();
        public List<ActionTakenVM> List { get; set; } = new List<ActionTakenVM>();
    }
}
