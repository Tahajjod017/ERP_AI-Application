using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class AddTeamVM : BaseViewModel
    {
        public int TeamID { get; set; }

        [Required(ErrorMessage = "{0} is Required"), DisplayName("Team Name")]
        public string TeamName { get; set; }
        public int EmpId { get; set; }
        public List<int>? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int TeamMemberID { get; set; }
        public string? GeneratedID { get; set; }

        //public virtual AspNetUsers CreatedByNavigation { get; set; }

        //public virtual ICollection<TeamMembersVM> TeamMembersVMs { get; set; } = new List<TeamMembersVM>();
    }
}
