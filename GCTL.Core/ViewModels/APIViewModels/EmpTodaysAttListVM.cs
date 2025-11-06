using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class EmpTodaysAttListVM
    {
        public int SlNo { get; set; }
        public string? AttendenceType { get; set; }
        public string? PunchTime { get; set; }
    }
}
