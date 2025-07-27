using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence
{
    public class AttendanceFilterViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Department { get; set; }
        public string PossibleReason { get; set; }
        public string DateRange { get; set; }
        public string Search { get; set; }
        public string Sort { get; set; }
    }


}
