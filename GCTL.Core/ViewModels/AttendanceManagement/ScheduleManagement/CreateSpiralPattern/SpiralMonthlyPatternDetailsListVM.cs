using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralMonthlyPatternDetailsListVM : BaseViewModel
    {
        public int SpiralMonthlyPatternDetailID { get; set; }

        public int? SpiralMonthlyPatternID { get; set; }

        public int? DayOfMonth { get; set; }

        public int? ShiftID { get; set; }

        public string ShiftTime { get; set; }
    }
}
