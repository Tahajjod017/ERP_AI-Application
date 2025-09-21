using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadActivityResultVM
    {
        public bool? IsWon { get; set; }   // single flag about the lead
        public List<LeadActivityVM> Activities { get; set; }
        public int? SuccessPercentage { get; set; }
        public int? LostPercentage { get; set; }
        public int? CancelPercentage { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
