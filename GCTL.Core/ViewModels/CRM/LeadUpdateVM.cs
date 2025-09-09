using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadUpdateVM : BaseViewModel
    {
        public int LeadID { get; set; }
        public string LeadName { get; set; }

        public int? LeadStatusID { get; set; }
        

        public int? LeadSourceID { get; set; }

        public int? LeadOwnerID { get; set; }
        public int? PriorityID { get; set; }

        public decimal? ApproximateDealValue { get; set; }

        public decimal? ProbabilityPercentage { get; set; }

        public string? LeadDescription { get; set; }
        public List<int?> ServiceTypeIds { get; set; }
    }
}
