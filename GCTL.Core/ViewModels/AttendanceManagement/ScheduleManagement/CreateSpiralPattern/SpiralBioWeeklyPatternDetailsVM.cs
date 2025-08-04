using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralBioWeeklyPatternDetailsVM : BaseViewModel
    {
        public int SpiralBioWeeklyPatternDetailID { get; set; }

        public int? SpiralBioWeeklyPatternID { get; set; }

        public int? DayOfMonth { get; set; }

        public int? ShiftID { get; set; }
    }
}
