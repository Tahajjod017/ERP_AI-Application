using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.LeadActivityType
{
    public class LeadActivityTypeVM : BaseViewModel
    {
        public int LeadActivityTypeID { get; set; }

        public string LeadActivityIcon { get; set; }

        public string LeadActivityName { get; set; }
        public string UseFor { get; set; }
    }
}
