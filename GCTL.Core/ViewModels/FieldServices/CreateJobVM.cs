using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
    public class CreateJobVM
    {
        public int CreateJobID { get; set; }
        public string JobTitle { get; set; }
        public int? ServiceID { get; set; }
        public List<int>? TeamMembers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusID { get; set; }
        public string? JobLocation { get; set; }
        public string? Note { get; set; }
        public string? file { get; set; }
    }
}
