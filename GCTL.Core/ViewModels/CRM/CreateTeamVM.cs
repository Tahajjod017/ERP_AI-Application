using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class CreateTeamVM : BaseViewModel
    {
        public int TeamID { get; set; }
        public string TeamName { get; set; }
        public List<int>? EmployeeIds { get; set; }
    }
}