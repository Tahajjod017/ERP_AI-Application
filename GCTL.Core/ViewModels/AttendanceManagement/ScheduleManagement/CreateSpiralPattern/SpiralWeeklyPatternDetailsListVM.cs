using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralWeeklyPatternDetailsListVM : BaseViewModel
    {
        public int SpiralWeeklyPatternDetailID { get; set; }

        public int? SpiralWeeklyPatternID { get; set; }

        public byte DayOfWeek { get; set; }

        public int? ShiftID { get; set; }

        public string ShiftTime { get; set; }
    }
}
