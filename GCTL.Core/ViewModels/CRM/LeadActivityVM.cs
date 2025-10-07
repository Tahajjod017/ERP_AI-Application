using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadActivityVM
    {
        public int LeadDetailID { get; set; }
        public DateTime? ActivityDateTime { get; set; }
        public string? ActivityNote { get; set; }
        public string? FileLink { get; set; }
        public string? LeadActivityName { get; set; }
        public string? LeadActivityIcon { get; set; }
        public string? CreatedByName { get; set; }
    }

}
