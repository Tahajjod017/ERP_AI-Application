using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class LeadDetailsVM : BaseViewModel
    {
        public int? LeadDetailID { get; set; }
        public int? LeadID { get; set; }

        public int? LeadActivityTypeID { get; set; }

        public DateTime? ActivityDateTime { get; set; }
        public string? ActivityDateTime2 { get; set; }

        public string ActivityNote { get; set; }
        public IFormFile? File { get; set; }
        public List<CommonSelectVM2>? ContactNumbers { get; set; }
        public List<CommonSelectVM2>? ContactEmails { get; set; }
        public bool? IsDone { get; set; }

    }

}
