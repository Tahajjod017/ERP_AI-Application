using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.Currencies;

namespace GCTL_App.ViewModels.AddTeam
{
    public class AddTeamPageVM
    {
        public AddTeamVM Setup { get; set; } = new AddTeamVM();
        public List<AddTeamVM> List { get; set; } = new List<AddTeamVM>();
    }
}
