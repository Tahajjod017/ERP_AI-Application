using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM.AddTeam
{
    public class TeamViewVM
    {
        public int TeamID { get; set; }
        public string TeamGID { get; set; }
        public string TeamName { get; set; }
        public List<TeamDetailsItemVM>? TeamDetails { get; set; }
    }

    public class TeamDetailsItemVM
    {
        public string? TeamMemberName { get; set; }
        public bool? IsTeamHead { get; set; }
    }


    public class TeamEditVM
    {
        public int TeamID { get; set; }
        public string TeamName { get; set; }
        public List<PertialEditVM>? TeamMembersInfo { get; set; }
    }

    public class PertialEditVM()
    {
        public int? TeamMemberID { get; set; }
        public string? TeamMemberName { get; set; }
    }
}
