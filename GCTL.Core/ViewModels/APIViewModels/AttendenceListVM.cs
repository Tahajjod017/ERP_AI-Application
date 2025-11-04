using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class AttendenceListVM
    {
        public int SlNo { get; set; }
        public string? AttendenceType { get; set; }
        public DateTimeOffset? PunchTime { get; set; }
        public string? PunchTimeFormatted { get; set; }
    }
}
