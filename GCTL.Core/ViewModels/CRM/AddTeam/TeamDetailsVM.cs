using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM.AddTeam
{
    public class TeamDetailsVM
    {
        public int TeamID { get; set; }
        public string TeamGID { get; set; }
        public string TeamName { get; set; }
        public List<TeamMemberDetails>? MemberDetails{ get; set; }
    }

    public class TeamMemberDetails
    {
        public string? TeamMemberName { get; set; }
        public int? EmployeeID { get; set; }
        public string? Designation { get; set; }
        public string? MobileNumber { get; set; }
        public string? profileImage { get; set; }
        public bool? IsTeamHead { get; set; }

    }
}
