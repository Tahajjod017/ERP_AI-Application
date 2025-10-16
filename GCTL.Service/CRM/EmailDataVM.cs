using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM
{
    public class TeamMemberDto
    {
        public int? ComapanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
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
    public class TeamPageMainDto
    {
        public List<TeamDto> Teams { get; set; }
        public List<ApplicationUser> AdminIds { get; set; }
        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationAddress { get; set; }
        public string? OrganizationEmail { get; set; }
        public string? OrganizationPhone { get; set; }
        public string? LogoLink { get; set; }
    }
}
