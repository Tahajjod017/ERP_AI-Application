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
        public List<string>? TeamMemberName { get; set; }
    }
}
