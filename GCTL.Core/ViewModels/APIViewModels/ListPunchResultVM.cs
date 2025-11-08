using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.APIViewModels
{
    public class ListPunchResultVM
    {
        public int InTime { get; set; }

        public int OutTime { get; set; }

        public ICollection<EmpTodaysAttListVM> AttendenceListVMs { get; set; } = new List<EmpTodaysAttListVM>();
    }
}
