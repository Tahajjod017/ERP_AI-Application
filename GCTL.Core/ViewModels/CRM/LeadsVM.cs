using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadsVM : BaseViewModel
    {
        public bool? IsIndividualCustomer { get; set; }
        public string LeadName { get; set; }

        public int LeadStatusID { get; set; }

        public int LeadSourceID { get; set; }

        public int LeadOwnerID { get; set; }

        public decimal? ApproximateDealValue { get; set; }

        public decimal? ProbabilityPercentage { get; set; }

        public string? LeadDescription { get; set; }
        public List<CustomerBaseVM> Customers { get; set; }

    }
}
