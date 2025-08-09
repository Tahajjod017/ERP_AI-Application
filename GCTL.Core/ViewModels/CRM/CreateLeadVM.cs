

using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Core.ViewModels.CRM
{
    public class CreateLeadVM
    {
        public IList<SelectListItem>  Services { get; set; }
        public IList<SelectListItem> LeadStatuses { get; set; }
        public IEnumerable<SelectListItem> LeadSources { get; set; }

        public Customers Customers { get; set; }
    }
}
