using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class IsWonVM : BaseViewModel
    {
        public int LeadID { get; set; }

        public int LeadActivityTypeID { get; set; }

        public string? ActivityNote { get; set; }
    }
}
