using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM
{
    public class TeamMemberDto
    {
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? LogoLink { get; set; }
        public int? LeadProjectTeamMemberID { get; set; }
        public string? LeadProjectTeamMemberName { get; set; }
        public string? LeadProjectTeamMemberEmail { get; set; }
        public bool? IsTeamHead { get; set; }
    }

    public class TeamDto
    {
        public int TeamID { get; set; }
        public string? TeamName { get; set; }
        public List<TeamMemberDto> TeamMembers { get; set; } = new();
    }
}
