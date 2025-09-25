using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.LeadStatuses
{
    public class LeadStatusVM : BaseViewModel
    {
        public int LeadStatusID { get; set; }

        public string LeadStatusName { get; set; }
        public bool? IsSpecial { get; set; }
    }
}
